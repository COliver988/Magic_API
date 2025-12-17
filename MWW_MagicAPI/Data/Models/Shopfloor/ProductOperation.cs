namespace MWW_Api.Models.Shopfloor;

public class ProductOperation
{
    public long Id { get; set; }
    public int CompanyID { get; set; }
    public long ProductId { get; set; }
    public int Index { get; set; }
    public long OperationId { get; set; }
    public decimal StandardTime { get; set; }
    public decimal PieceRate { get; set; }
    public bool WorkorderLevel { get; set; }
    public bool UserLevel { get; set; }
    public DateTime Created { get; set; }
    public bool WipEnd { get; set; }
    public string? Name { get; set; }
    public bool QcControl { get; set; }
    public int? SubRouteId { get; set; }
    public int DefaultQuantity { get; set; }
    public int? ExternalSeqNo { get; set; }
    public decimal AddOnTime { get; set; }
    public int? BaseRateId { get; set; }
    public decimal EcAddOnTime { get; set; }
    public int? MileStoneId { get; set; }
    public bool MileStoneTrigger { get; set; }
    public int? LastBaseRateId { get; set; }
    public string? NotesHeader { get; set; }
    public string? Notes { get; set; }
    public string? TabletEditableNotes { get; set; }
    public string? Comments { get; set; }
    public string? UserName { get; set; }
    public int? Panels { get; set; }

}
