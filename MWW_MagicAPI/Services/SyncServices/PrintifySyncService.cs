using Microsoft.EntityFrameworkCore;
using MWW_Api.Models.Magic;
using MWW_Api.Models.Peeps.Printify;
using MWW_MagicAPI.Data.Contexts;
using MWW_MagicAPI.Data.Models.DTO;
using MWW_MagicAPI.Data.RepositoryContracts.Peeps.Printify;

namespace MWW_MagicAPI.Services.SyncServices;

public class PrintifySyncService : ISyncService
{
    private readonly PeepsDbContext _context;
    private readonly IPrintifyOrderRepository _orderRepository;
    private readonly IPrintifyEventRepository _eventRepository;
    private readonly ILogger<PrintifySyncService> _logger;
    private readonly HttpClient _httpClient;
    private enum PrintifyStatuses
    {
        created,
        picked,
        printed,
        packaged,
        shipped,
        cancelled
    }

    public PrintifySyncService(
        PeepsDbContext context,
        IPrintifyOrderRepository orderRepository,
        IPrintifyEventRepository eventRepository,
        HttpClient httpClient,
        ILogger<PrintifySyncService> logger)
    {
        _context = context;
        _orderRepository = orderRepository;
        _eventRepository = eventRepository;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<SyncDataResults>> SyncData(
        List<UpdateData> data,
        List<MilestoneMapper> milestoneMappings)
    {
        if (data == null || data.Count == 0)
            return new List<SyncDataResults>();

        // remove any non-Printify POs
        data = await FilterPrintifyOrders(data);
        return await UpdatePrintifyStatuses(data, milestoneMappings);
    }

    private async Task<List<UpdateData>> FilterPrintifyOrders(List<UpdateData> data)
    {
        List<string> pos = GetDistinctPOs(data);
        if (pos == null || pos.Count == 0)
            return new List<UpdateData>();

        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        const int chunkSize = 100;
        for (int i = 0; i < pos.Count; i += chunkSize)
        {
            var chunk = pos.Skip(i).Take(chunkSize).ToList();
            var matched = await _context.PrintifyOrders
                .AsNoTracking()
                .Where(o => chunk.Contains(o.UniqueId))
                .Select(o => o.UniqueId)
                .ToListAsync();

            foreach (var id in matched)
                found.Add(id?.Trim() ?? string.Empty);
        }

        // preserve original ordering and return only updates that exist in PrintifyOrders
        return data
            .Where(d => !string.IsNullOrWhiteSpace(d.VendorPO) && found.Contains(d.VendorPO.Trim()))
            .ToList();
    }

    private List<string>? GetDistinctPOs(List<UpdateData> data)
    {
        List<string> results = new List<string>();
        if (data != null || data.Count > 0)
        results = data
            .Select(d => d.VendorPO?.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        return results;
    }

    private async Task<List<SyncDataResults>> UpdatePrintifyStatuses(
        List<UpdateData> updates,
        List<MilestoneMapper> mappings)
    {
        List<SyncDataResults> updated = new();

        foreach (UpdateData update in updates)
            {
                string status = mappings
                    .Where(m => m.FS_Status.Equals(update.FS_Status, StringComparison.OrdinalIgnoreCase))
                    .Select(m => m.PrintifyStatus)
                    .FirstOrDefault() ?? string.Empty;
                updated.AddRange(await ProcessUpdate(update, status));
            }

        return updated.Where(r => r != null).ToList();
    }

    /// <summary>
    /// process the update; wrapped in transaction so it all works or not
    /// </summary>
    /// <param name="po"></param>
    /// <param name="status">may be a comma-separated list of statuses</param>
    /// <returns>true if success, false if not</returns>
    /// TODO: if it fails, how do we handle the update later? missed updates?
    private async Task<List<SyncDataResults>?> ProcessUpdate(UpdateData update, string statuses)
    {
        List<SyncDataResults> results = new();
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (string status in statuses.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                bool added = await _orderRepository.UpdateAsync(update.VendorPO, status);
                if (added)
                {
                    List<PrintifyEvent> events = await CreateEvents(update.VendorPO, status);
                    await transaction.CommitAsync();
                    results = events.Select(e => new SyncDataResults
                    {
                        VendorPO = update.VendorPO,
                        PO = update.SerialNumber,
                        LnNo = 0,
                        NewStatus = status,
                        RecordType = "PrintifyEvent"
                    }).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing Printify order update for PO {update.VendorPO} to status {statuses}");
            await transaction.RollbackAsync();
            results = new List<SyncDataResults>();
        }
        return results;
    }

    /// <summary>
    /// create the evebnt for the PO; may have to create earlier events if missing
    /// </summary>
    /// <param name="po"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    private async Task<List<PrintifyEvent>> CreateEvents(string po, string status)
    {
        List<PrintifyEvent> addedEvents = new List<PrintifyEvent>();
        PrintifyOrder? order = await _orderRepository.GetByOrderPOAsync(po);
        if (order == null) return addedEvents;
        List<PrintifyEvent> events = await _eventRepository.GetAllByOrder(order.Id);
        if (events.Any(e => e.Action == status))
            return addedEvents; // event already exists
        addedEvents = GenerateEvents(order, events, status);
        addedEvents = await _eventRepository.AddEvents(addedEvents);
        return addedEvents;
    }

    /// <summary>
    /// create a list of events for this status and any previous that are missing
    /// </summary>
    /// <param name="order"></param>
    /// <param name="existingEvents"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    private async Task<List<PrintifyEvent>> GenerateEvents(PrintifyOrder order, List<PrintifyEvent> existingEvents, string status)
    {
        List<PrintifyEvent> events = new List<PrintifyEvent>();
        Array statuses = Enum.GetValues(typeof(PrintifyStatuses));
        int targetIndex = FindTargetIndex(statuses, status);
        for (int i = 0; i <= targetIndex; i++)
        {
            string currentStatus = statuses.GetValue(i)!.ToString()!;
            if (existingEvents.Any(e => e.Action == currentStatus))
                continue; // already have this event
            events.Add(new PrintifyEvent()
            {
                OrderId = order.Id,
                Action = currentStatus,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AffectedItems = order.PrintifyItems.Select(i => i.UniqueId).ToArray()
            });
        }
        if (status == PrintifyStatuses.shipped.ToString())
            await CheckNotifications(order, events, existingEvents);
        return events;
    }

    /// <summary>
    /// check for events; we may update an existing shipped event
    /// </summary>
    /// <param name="order"></param>
    /// <param name="events"></param>
    /// <param name="existingEvents"></param>
    /// <returns></returns>
    private async Task CheckNotifications(PrintifyOrder order, List<PrintifyEvent> events, List<PrintifyEvent> existingEvents)
    {
        // find the shipped event we just added
        var shippedEvent = events.FirstOrDefault(e => e.Action == PrintifyStatuses.shipped.ToString());
        if (shippedEvent == null)
            shippedEvent = existingEvents.FirstOrDefault(e => e.Action == PrintifyStatuses.shipped.ToString());
        if (shippedEvent != null)
        {
            // send notification to client
            _ = SendNotifications(order, shippedEvent);
        }
    }

    /// <summary>
    /// send the client notifications if we have shipping info; should also fill in event details
    /// </summary>
    /// <param name="order">Printify order</param>
    /// <returns></returns>
    private async Task SendNotifications(PrintifyOrder order, PrintifyEvent shippedEvent)
    {
        //TODO: send notification to client that order has shipped IF we have the Exenta tracking number
        await Task.CompletedTask;
    }

    /// <summary>
    /// find the position for this enum value
    /// </summary>
    /// <param name="statuses"></param>
    /// <param name="status"></param>
    /// <returns>index of enum else -1 if invalid</returns>
    private int FindTargetIndex(Array statuses, string status)
    {
       	if (!Enum.TryParse<PrintifyStatuses>(status, true, out var parsed)) return -1;
        int targetIndex = Array.IndexOf(statuses, Enum.Parse(typeof(PrintifyStatuses), status));
        return targetIndex;
    }
}
