namespace MWW_Api.Models.Magic;

public class DyePrintDetails
{
    public required string CO_Number { get; set; }
    public short Ln_No { get; set; }
    public required string PO { get; set; }
    public string? TemplateID { get; set; }
    public string? PrinterID { get; set; }
    public required string Status { get; set; }
    public string? JobTicketFileName { get; set; }
    public DateTime? PrintedDate { get; set; }
    public string? BatchID { get; set; }
    public short? PrintOrder { get; set; }
    public string? AssignMode { get; set; }
    public DateTime? AssignModeTime { get; set; }
    public bool? isOpenSubGroup { get; set; }
    public short? ShipInFS { get; set; }
    public string? PrunNumber { get; set; }
    public int? printedOrder { get; set; }
    public string? PrintLocation { get; set; }
}
