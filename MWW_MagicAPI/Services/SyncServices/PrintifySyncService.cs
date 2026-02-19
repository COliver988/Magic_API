using Microsoft.EntityFrameworkCore;
using MWW_Api.Models.Magic;
using MWW_Api.Models.Peeps.Printify;
using MWW_Api.Services.Peeps.PrintifyServices;
using MWW_MagicAPI.Data.Contexts;
using MWW_MagicAPI.Data.Models.DTO;
using MWW_MagicAPI.Data.RepositoryContracts.Peeps.Printify;
using static MWW_Api.Models.Peeps.Printify.PrintifyShared;

namespace MWW_MagicAPI.Services.SyncServices;

public class PrintifySyncService : ISyncService
{
    private readonly PeepsDbContext _context;
    private readonly IPrintifyOrderRepository _orderRepository;
    private readonly IPrintifyEventRepository _eventRepository;
    private readonly ILogger<PrintifySyncService> _logger;
    private readonly IPrintifyHttpClientService _printifyHttpClient;

    public PrintifySyncService(
        PeepsDbContext context,
        IPrintifyOrderRepository orderRepository,
        IPrintifyEventRepository eventRepository,
        IPrintifyHttpClientService printifyHttpClient,
        ILogger<PrintifySyncService> logger)
    {
        _context = context;
        _orderRepository = orderRepository;
        _eventRepository = eventRepository;
        _printifyHttpClient = printifyHttpClient;
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
            string? newStatus = mappings.FirstOrDefault(s => s.FS_Status == update.FS_Status)?.PrintifyStatus!;
            if (newStatus != null)
            {
                if (await ProcessUpdate(update.VendorPO, newStatus, update))
                {
                    updated.Add(new SyncDataResults
                    {
                        VendorPO = update.VendorPO,
                        PO = update.SerialNumber,
                        LnNo = 0,
                        NewStatus = newStatus,
                        RecordType = "PrintifyEvent"
                    });
                }
            }
        }

        return updated;
    }

    /// <summary>
    /// process the update; wrapped in transaction so it all works or not
    /// </summary>
    /// <param name="po"></param>
    /// <param name="status"></param>
    /// <returns>true if success, false if not</returns>
    /// TODO: if it fails, how do we handle the update later? missed updates?
    private async Task<bool> ProcessUpdate(string po, string status, UpdateData update)
    {
        bool results = true;
        string[] statuses = status.Split(',');
        await using var transaction = await _context.Database.BeginTransactionAsync();
        PrintifyOrder? order = await _orderRepository.GetByOrderPOAsync(po);
        if (order == null) return false;

        try
        {
            foreach (string newStatus in statuses)
            {
                List<PrintifyEvent> addedEvents = await CreateEvents(order, status, update);
                results = await SendNotifications(order, addedEvents);
                if (results)
                    results = await _orderRepository.UpdateAsync(order.UniqueId, addedEvents.LastOrDefault()?.Action ?? newStatus.Trim());
            }
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing Printify order update for PO {po} to status {status}");
            await transaction.RollbackAsync();
            results = false;
        }
        return results;
    }

    /// <summary>
    /// create the event for the PO; may have to create earlier events if missing
    /// </summary>
    /// <param name="order">printify order</param>
    /// <param name="status">specific status</param>
    /// <returns>list of Pritify events added</returns>
    private async Task<List<PrintifyEvent>> CreateEvents(PrintifyOrder order, string status, UpdateData update)
    {
        List<PrintifyEvent> events = await _eventRepository.GetAllByOrder(order.Id);
        if (events.Any(e => e.Action == status))
            return new List<PrintifyEvent>(); // event already exists
        List<PrintifyEvent> newEvents = GenerateEvents(order, events, status, update);
        List<PrintifyEvent> addedEvents = await _eventRepository.AddEvents(newEvents);
        return addedEvents;
    }

    /// <summary>
    /// send the client notifications
    /// </summary>
    /// <param name="addedEvents"></param>
    /// <returns></returns>
    private async Task<bool> SendNotifications(PrintifyOrder order, List<PrintifyEvent> addedEvents)
    {
        bool results = true;
        foreach (PrintifyEvent printifyEvent in addedEvents)
        {
            results = await _printifyHttpClient.SendPrintifyUpdateAsync(order, printifyEvent.Action);
            if (!results) break;
        }
        return results;
    }

    /// <summary>
    /// create a list of events for this status and any previous that are missing
    /// </summary>
    /// <param name="order"></param>
    /// <param name="existingEvents"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    private List<PrintifyEvent> GenerateEvents(PrintifyOrder order, List<PrintifyEvent> existingEvents, string status, UpdateData update)
    {
        List<PrintifyEvent> events = new List<PrintifyEvent>();
        Array statuses = Enum.GetValues(typeof(PrintifyStatuses));
        int targetIndex = FindTargetIndex(statuses, status);
        for (int i = 0; i <= targetIndex; i++)
        {
            string currentStatus = statuses.GetValue(i)!.ToString()!;
            if (existingEvents.Any(e => e.Action == currentStatus))
                continue; // already have this event
            var details = "{}";
            if (status == "shipped")
                details = new { TrackingNumber = update.TrackingInfo, Carrier = update.FS_Carrier }.ToString() ?? "{}";
            events.Add(new PrintifyEvent()
            {
                OrderId = order.Id,
                Action = currentStatus,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AffectedItems = order.PrintifyItems.Select(i => i.UniqueId).ToArray(),
                Details = details
            });
        }
        return events;
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
