namespace MWW_MagicAPI.Data.Models.Magic;

public class DyePrintHeader
{
    public string CO_Number { get; set; }
    public string PO { get; set; }
    public string? OrderID { get; set; }
    public string? ItemSize { get; set; }
    public string? ItemCode { get; set; }
    public string SubGroupId { get; set; }
    public short PriorityCode { get; set; }
    public string? PrintStatus { get; set; }
    public short OriginalQty { get; set; }
    public string? WhatLblToPrint { get; set; }
    public DateTime? created_Date { get; set; }
    public DateTime? PromShipDate { get; set; }
    public string? SectionType { get; set; }
    public string? BinID { get; set; }
    public string? PrintLocation { get; set; }
    public string? CertField { get; set; }
    public string? MOSchedDate { get; set; }
    public string? MONumber { get; set; }
    public string? DTGXML { get; set; }
}
