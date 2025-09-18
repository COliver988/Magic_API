namespace MWW_MagicAPI.Data.Models.DTO;

public record WorkOrderDataDTO
{
    public int ProdStageNo { get; set; }
    public int ProdNoCompany { get; set; }
    public int OpenSeq { get; set; }
    public int ItemNo { get; set; }
    public string Style { get; set; }
    public string StyleName { get; set; }
    public string Label { get; set; }
    public string Color { get; set; }
    public string ColorDesc { get; set; }
    public string Dimension { get; set; }
    public string DimensionDesc { get; set; }
    public string Size { get; set; }
    public string SizeDesc { get; set; }
    public int? ProdLineQty { get; set; }
    public string UOM { get; set; }
    public string DetailShipDate { get; set; }
    public string DtlDueDate { get; set; }
    public int OrderNoCompany { get; set; }
    public string PONumber { get; set; }
    public string Warehouse { get; set; }
    public string Consolidate { get; set; }
    public string Message { get; set; }
}
