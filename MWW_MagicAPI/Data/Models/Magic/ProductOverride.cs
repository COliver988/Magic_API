using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Magic;

[Table("product_overrdes")]
public class ProductOverride
{
    public ProductOverride() { }
    [Column("id")]
    public long Id { get;set ; }

    [Column("product_code")]
    public string ProductCode {  get; set; }

    [Column("tag_override")]
    public string TagOverrde {  get; set; }

    [Column("override_type")]
    public long OverrideType {  get; set; }
}
