using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Peeps.Printify;

[Table("printify_shipping_methods")]
public class PrintifyShippingMethod
{
    [Column("id")]
    public long Id { get; set; }

    [Column("Carrier")]
    public string? Carrier { get; set; }

    [Column("priority")]
    public string? Priority { get; set; }

    [Column("created_at")]
    [DisplayName("Created At")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [DisplayName("Updated At")]
    public DateTime UpdatedAt { get; set; }
}
