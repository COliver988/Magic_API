using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Peeps.Printify;

[Table("printify_images")]
public class PrintifyImage
{
    [Column("id")]
    public long Id { get; set; }

    [Column("item_id")]
    public Int64 ItemId { get; set; }

    [Column("print_location")]
    public string? PrintLocation { get; set; }

    [Column("type")]
    public string? Type { get; set; }

    [Column("url")]
    public string? Url { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

}
