using Newtonsoft.Json;

namespace MWW_Api.Models.Peeps.Printify;
public record PrintifyUpdatePayload
{
    [JsonProperty("event_id")]
    public required string EventId { get; set; }

    [JsonProperty("facility_id")]
    public required string FacilityId { get; set; }

    [JsonProperty("occurred_at")]
    public required DateTime OccurredAt { get; set; }

    [JsonProperty("type")]
    public required string Type { get; set; }

    [JsonProperty("affected_item")]
    public required string AffectedItem { get; set; }
}
