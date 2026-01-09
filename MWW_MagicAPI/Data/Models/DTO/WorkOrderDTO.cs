namespace MWW_MagicAPI.Data.Models.DTO;

public record WorkOrderUnitData
{
    public string Workorder { get; set; }
    public string Batch { get; set; }
    public int Seq { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; }
    public string Thumbnail { get; set; }
    public string Content { get; set; }
    public string Flag { get; set; }
    public string ProductId { get; set; }
    public string Size { get; set; }
    public string SizeDesc { get; set; }
}

public record MagicUnit
{
    public int ProdNoCompany { get; set; }
    public int? OpenSeq { get; set; }
    public string BatchID { get; set; }
    public short? PrintOrder { get; set; }
}