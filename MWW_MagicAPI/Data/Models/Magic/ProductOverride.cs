using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Magic;

[Table("product_overrdes")]
public class ProductOverride
{
    public ProductOverride() { }
    [Key]
    [Column("id")]
    public int Id { get;set ; }

    [Column("product_code")]
    public string ProductCode {  get; set; }

    [Column("tag_override")]
    public string TagOverrde {  get; set; }

    [Column("override_type")]
    public int OverrideType {  get; set; }
}
