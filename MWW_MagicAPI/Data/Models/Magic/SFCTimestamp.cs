using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Magic;

public class SFCTimestamp
{
    [Column("id")]
    public int Id { get; set; }
    public string? Location { get; set; }
    public DateTime? LastChecked { get; set; }
}
