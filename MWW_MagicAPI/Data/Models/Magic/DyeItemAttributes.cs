using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_MagicAPI.Data.Models.Magic;

[Table("dyeitem_Attributes")]
public class DyeItemAttributes
{
    public string MWWItemCode { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? Style { get; set; }
    public string? Label { get; set; }
    public string? Dimensions { get; set; }
    public string? Prepack { get; set; }
    public string? GL_SalesAcct { get; set; }
    public bool? Active { get; set; }
    public int? ITEMNO { get; set; }
    public string? UPC { get; set; }
    public string? UOMBASE { get; set; }
    public decimal? PRICEWS { get; set; }
    public string? ProdType { get; set; }
}
