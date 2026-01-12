using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Peeps.Printify;

[Table("printify_products")]
public class PrintifyProducts
{
    [Column("id")]
    public long Id { get; set; }

    [Column("sku")]
    public string? Sku { get; set; }

    [Column("mww_product_code")]
    public string? MwwProductCode { get; set; }

    [Column("stock")]
    public int? Stock { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("blank_price")]
    public int? BlankPrice { get; set; }

    [Column("processing_price")]
    public int? ProcessingPrice { get; set; }

    [Column("printing_price")]
    public int? PrintingPrice { get; set; }

    [Column("country_code")]
    public int? CountryCode { get; set; }
}
