using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Peeps.Printify;

[Table("printify_items")]
public class PrintifyItem
{
    [Column("id")]
    public long Id { get; set; }

    [Column("order_id")]
    public Int64 OrderId { get; set; }

    [Column("unique_id")]
    public string UniqueId { get; set; }

    [Column("product_id")]
    public Int64 ProductId { get; set; }

    [Column("quantity")]
    public Int32 Quantity { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]  
    public DateTime UpdatedAt { get; set; }

    [Column("item_properties", TypeName = "jsonb")]
    public string? ItemProperties { get; set; }
}
