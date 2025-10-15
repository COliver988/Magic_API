namespace MWW_Api.Models.Magic;

public class ExentaPOLinesWithAckNo
{
    public decimal RecId { get; set; }
    public string PO { get; set; }
    public int LN_NO { get; set; }
    public string? OrderNoCompany { get; set; }
    public string? ProdNoCompany { get; set; }
    public int? OpenSeq { get; set; }
    public string? PickNo { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? UpdateAckFilename { get; set; }

}
