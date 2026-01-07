using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Peeps.Printify;

[Table("printify_orders")]
public class PrintifyOrder
{
    [Column("id")]
    public long Id { get; set; }

    [Column("unique_id")]
    public string UniqueId { get; set; }

    [Column("sample")]
    public bool? Sample { get; set; }

    [Column("reprint")]
    public bool? Reprint { get; set; }

    [Column("xqc")]
    public bool? Xqc { get; set; }

    [Column("status")]
    public string? Status { get; set; }


    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("shipping_method_id")]
    public Int64? ShippingMethodId { get; set; }

    [Column("tags", TypeName = "jsonb")]
    public string? Tags { get; set; }
}
