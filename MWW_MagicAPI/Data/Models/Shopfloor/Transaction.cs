using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Shopfloor;

public class Transaction
{
    public long Id { get; set; }
    public int StartEvent { get; set; }
    public string? EventName { get; set; }

    [Column(TypeName = "decimal(14,6)")]
    public decimal PieceRate { get; set; }

    [Column(TypeName = "decimal(10,6)")]
    public decimal StandardTime { get; set; }

    [Column(TypeName = "decimal(14,6)")]
    public decimal PaymentRate { get; set; }
    public int CompanyId { get; set; }
    public DateTime DateTime { get; set; }
	public long UserId { get; set; }
    public long LocationId { get; set; }
    public long OperationId { get; set; }
    public long WorkorderId { get; set; }
    public long UnitId { get; set; }
    public int ProductionCount { get; set; }
    public int CycleTime { get; set; }
    public bool IsOpen { get; set; }
    public bool IsPause { get; set; }
    public bool IsProduction { get; set; }
    public bool IsOffstandard { get; set; }
    public int? OffstandardId { get; set; }
    public bool IsDeviation { get; set; }
    public int? DeviationId { get; set; }
    public bool IsRepair { get; set; }
    public bool IsQc { get; set; }
    public bool IsQcOfRepair { get; set; }
    public bool IsModification { get; set; }
    public DateTime Created { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime? IntegrationPostTime { get; set; }
    public int? IntegrationPostResult { get; set; }
    public string? IntegrationReturnCodes { get; set; }
    public int OperationIndex { get; set; }
    public string? OffStandardApprovedBy { get; set; }
    public bool OffStandardRequiresApproval { get; set; }
    public bool? OffStandardApproved { get; set; }
    public long ProductionIntervalId { get; set; }

    [Column(TypeName = "decimal(14,6)")]
    public decimal ProductionFraction { get; set; }
    public long ExtraInfoLong { get; set; }
    public string? ExtraInfoString { get; set; }
    public string? ExtraInfoString2 { get; set; }
    public bool IsAutomatic { get; set; }
    public string? ModifiedBy { get; set; }
    public string? ModifiedComment { get; set; }
    public int? ModifiedField { get; set; }
    public string? GUID { get; set; }
    public int? SyncPostResult { get; set; }
    public int? DepartmentId { get; set; }

    [Column(TypeName = "decimal(10,6)")]
    public decimal AddOnTime { get; set; }

    [Column(TypeName = "decimal(10,6)")]
    public decimal EcAddOnTime { get; set; }

    [Column(TypeName = "decimal(10,6)")]
    public decimal ExtraAddOnTime { get; set; }
    public long OriginalId { get; set; }
    public DateTime LocalDate { get; set; }
    public DateTime? OriginalDateTime { get; set; }
    public DateTime? OriginalEndTime { get; set; }
    public string? OverTimeApprovedBy { get; set; }
    public bool? IsOverTime { get; set; }
    public int? LinkedFromOffstandardId { get; set; }
    public int AuditCount { get; set; }

    [Column(TypeName = "decimal(14,6)")]
    public decimal AuditFraction { get; set; }
    public int? BaserateId { get; set; }
    public long QcByUserId { get; set; }
    public bool? AuditPassed { get; set; }
    public DateTime? ModifiedDateTime { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CycleTimeFactor { get; set; }
    public bool? FirstInDay { get; set; }
    public bool? LastInDay { get; set; }
    public bool? FirstUserInShare { get; set; }

    [Column(TypeName = "decimal(14,6)")]
    public decimal? ProductionUOM { get; set; }
    public int? UnitOfMeasure { get; set; }

    [Column(TypeName = "decimal(14,6)")]
    public decimal? AuditUOM { get; set; }
    public bool? IsWIP { get; set; }
    public bool? Aggregated { get; set; }
    public int? QcPartId { get; set; }
    public int? QualifiedOverTimeMinutes { get; set; }
    public DateTime? LocalDateTime { get; set; }
    public string? SegmentKey { get; set; }

    [Column(TypeName = "decimal(10,6)")]
    public decimal? ModuleSAM { get; set; }
    public string? Contract { get; set; }
    public string? TimeZone { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OnStandardCycleTimeFactor { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OnStandardSamFactor { get; set; }
    public string? ShareID { get; set; }
}
