using MWW_Api.Models.Magic;
using MWW_Api.Models.Peeps.Printify;
using MWW_MagicAPI.Data.Contexts;
using MWW_MagicAPI.Data.Models.DTO;
using MWW_MagicAPI.Data.RepositoryContracts.Peeps.Printify;
using MWW_MagicAPI.Services.SyncServices;

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

    public async Task<int> SyncData(
        List<UpdateData> data,
        List<MilestoneMapper> milestoneMappings)
    {
        if (data == null || data.Count == 0)
            return 0;

        return await UpdatePrintifyStatuses(data, milestoneMappings);
    }

    private async Task<int> UpdatePrintifyStatuses(
        List<UpdateData> updates,
        List<MilestoneMapper> mappings)
    {
        int updated = 0;

        foreach (UpdateData update in updates)
        {
            MilestoneMapper? mapped = mappings.FirstOrDefault(m =>
                m.NewStatus == update.MilestoneName &&
                !string.IsNullOrEmpty(m.PrintifyStatus));

            if (mapped == null) continue;

            if (await ProcessUpdate(update.SerialNumber, mapped.PrintifyStatus!))
                updated++;
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
    private async Task<bool> ProcessUpdate(string po, string status)
    {
        bool results = true;
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            results = await _orderRepository.UpdateAsync(po, status);
            if (results)
            {
                results = await CreateEvents(po, status);
                await transaction.CommitAsync();
            }
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
    /// create the evebnt for the PO; may have to create earlier events if missing
    /// </summary>
    /// <param name="po"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    private async Task<bool> CreateEvents(string po, string status)
    {
        PrintifyOrder? order = await _orderRepository.GetByOrderPOAsync(po);
        if (order == null) return false;
        List<PrintifyEvent> events = await _eventRepository.GetAllByOrder(order.Id);
        if (events.Any(e => e.Action == status))
            return true; // event already exists
        List<PrintifyEvent> newEvents = GenerateEvents(order, events, status);
        List<PrintifyEvent> addedEvents = await _eventRepository.AddEvents(newEvents);
        return await SendNotifications(addedEvents);
    }

    /// <summary>
    /// send the client notifications
    /// </summary>
    /// <param name="addedEvents"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task<bool> SendNotifications(List<PrintifyEvent> addedEvents)
    {
        return true;
    }

    /// <summary>
    /// create a list of events for this status and any previous that are missing
    /// </summary>
    /// <param name="order"></param>
    /// <param name="existingEvents"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    private List<PrintifyEvent> GenerateEvents(PrintifyOrder order, List<PrintifyEvent> existingEvents, string status)
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
                AffectedItems = order.PrintifyItems.Select(i => i.UniqueId).ToArray()
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
