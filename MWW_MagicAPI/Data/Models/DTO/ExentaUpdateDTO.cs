namespace MWW_MagicAPI.Data.Models.DTO;

// data from SFC DBs
public record UpdateData
{
    public string AlphaNumId { get; set; }
    public string MilestoneName { get; set; }
    public long OperationId { get; set; }
    public long ProductId { get; set; }
    public string SerialNumber { get; set; }

    // original vendor PO
    public string VendorPO { get; set; }

    // DAP_Partner F5 status
    public string LegacyStatus { get; set; }
    public DateTime Created { get; set; }
}

// Magic order data
public record LegacyData
{
    public string Po { get; set; }
    public string Co { get; set; }
    public string LnNo { get; set; }
    public string Status { get; set; }
    public string BatchSeq { get; set; }
    public string UserId { get; set; }
    public string LineNumber { get; set; }
}
