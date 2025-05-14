using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Magic;

public partial class StuckProductionOrders
{
    public StuckProductionOrders() { }

    [Key, Column("po")]
    public string PO {  get; set; }

    [Column("last_moved")]
    public DateTime LastMoved {  get; set; }

    [Column("date_due")]
    public DateTime DueDate { get; set; }

    [Column("status")]
    public string? Status { get; set; }
}
