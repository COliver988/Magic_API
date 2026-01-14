using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Peeps.Printify;

[Table("printify_events")]
public class PrintifyEvent
{
    [Column("id")]
    public long Id { get; set; }

    [Column("order_id")]
    public long OrderId { get; set; }

    [Column("action")]
    public string? Action { get; set; }

    [Column("affected_items", TypeName = "varchar[]")]
    public string[]? AffectedItems { get; set; }

    [Column("created_at")]

    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("details", TypeName = "jsonb")]
    public string? Details { get; set; }
}
