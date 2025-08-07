using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Magic;

[Table("undefined_products")]
public class UndefinedProduct
{
    [Column("id")]
    public int Id { get; set; }

    [Column("product_code")]
    public string ProductCode { get; set; }

    [Column("customer_id")]
    public string CustomerId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("occurences")]
    public int Occurences { get; set; }

    [Column("vendor_po")]
    public string VendorPo { get; set; }

    [Column("interface_id")]
    public int InterfaceId { get; set; }
}
