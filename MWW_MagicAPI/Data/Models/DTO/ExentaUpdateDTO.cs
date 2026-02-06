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
    public string FS_Status { get; set; }
    public string? TrackingInfo { get; set; }
    public DateTime Created { get; set; }
}

// Magic order data
public record LegacyData
{
    public string Po { get; set; }
    public string Co { get; set; }
    public int LnNo { get; set; }
    public string Status { get; set; }
    public string BatchSeq { get; set; }
    public string UserId { get; set; }
    public string LineNumber { get; set; }
}

public record SyncDataResults
{
    public string PO { get; set; }
    public string VendorPO { get; set; }
    public int LnNo { get; set; }
    public string RecordType { get; set; }
    public string OldStatus { get; set; }
    public string NewStatus { get; set; }
}
