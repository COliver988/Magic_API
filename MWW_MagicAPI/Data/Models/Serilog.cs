using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_MagicAPI.Models.Logging;

[Table("SeriLog")]
public partial class SeriLog
{
    public int Id { get; set; }

    public int? Level { get; set; }

    public DateTime? TimeStamp { get; set; }

    [Column(TypeName = "jsonb")]
    public LogEvent? LogEvent { get; set; }
}

public class LogEvent
{
    public string? Level { get; set; }
    public string? SpanId { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; }
    public EventProperties? Properties { get; set; }
}

public class EventProperties
{
    public string? UserName { get; set; }
    public string? RequestId { get; set; }
    public string? Application { get; set; }
    public string? ActionName { get; set; }
    public string? RequestPath { get; set; }
    public string? ConnectionId { get; set; }
    public string? SourceContext { get; set; }
    public string? TraceId { get; set; }
}