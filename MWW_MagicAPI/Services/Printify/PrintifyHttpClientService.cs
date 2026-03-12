using Newtonsoft.Json;
using MWW_Api.Models.Peeps.Printify;

namespace MWW_Api.Services.Peeps.PrintifyServices;

public class PrintifyHttpClientService : IPrintifyHttpClientService
{
    private readonly HttpClient _httpClient;
    private ILogger<PrintifyHttpClientService> _logger;
    public PrintifyHttpClientService(HttpClient httpClient,
        ILogger<PrintifyHttpClientService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> SendPrintifyUpdateAsync(PrintifyOrder order, string status)
    {
        List<PrintifyUpdatePayload> payload = GeneratePayload(order, status);
        string jsonPayload = JsonConvert.SerializeObject(payload);
        var url = _httpClient.BaseAddress!.ToString().Replace("{PRINTIFY-ORDER-ID}", order.UniqueId);
        var results = await _httpClient.PostAsJsonAsync(url, payload);
        if (results.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            // Log the error details here for debugging
            string errorContent = results.Content.ReadAsStringAsync().Result;
            _logger.LogError($"Failed to send Printify update. Status Code: {results.StatusCode}, Response: {errorContent}");
            return false;
        }
    }

    private List<PrintifyUpdatePayload> GeneratePayload(PrintifyOrder order, string status)
    {
        DateTime now = DateTime.UtcNow;
        List<PrintifyUpdatePayload> payload = new List<PrintifyUpdatePayload>();
        foreach (PrintifyItem item in order.PrintifyItems)
        {
            payload.Add(new PrintifyUpdatePayload
            {
                EventId = $"{order.UniqueId}_{item.Id}_{status}",
                FacilityId = PrintifyShared.FacilityName,
                OccurredAt = now,
                Type = status,
                AffectedItem = item.Id.ToString()
            });
        }
        return payload;
    }
}