using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Shopfloor;

public class Unit
{
    public long Id { get; set; }
    public long WorkorderId { get; set; }
    public int Quantity { get; set; }
    public int TotalProductionStartedCount { get; set; }
    public int TotalProductionFinishedCount { get; set; }
    public int TotalOnStandardProductionCount { get; set; }
    public int TotalOffStandardProductionCount { get; set; }
    public int TotalOnStandardDeviationCount { get; set; }
    public int TotalOffStandardDeviationCount { get; set; }

    [Column(TypeName = "decimal(12,6)")]
    public decimal TotalOnStandardSMV { get; set; }

    [Column(TypeName = "decimal(12,6)")]
    public decimal TotalOffstandardSMV { get; set; }

    [Column(TypeName = "decimal(12,6)")]
    public decimal TotalOnStandardDeviationSMV { get; set; }

    [Column(TypeName = "decimal(12,6)")]
    public decimal TotalOffstandardDeviationSMV { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal TotalMonetaryValue { get; set; }

    public int TotalOnStandardCycleTime { get; set; }
    public int TotalOffStandardCycleTime { get; set; }
    public int TotalPauseCycleTime { get; set; }
    public int CompanyId { get; set; }
    public string AlphaNumId { get; set; }
    public bool Active { get; set; }
    public DateTime Created { get; set; }
    public int Index { get; set; }
    public DateTime? LastAccumulation { get; set; }
    public int TotalScrapCount { get; set; }
    public string? ExternalUnit { get; set; }
    public string? GUID { get; set; }
    public DateTime? Modified { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? FirstAccumulation { get; set; }
    public int TotalAuditCount { get; set; }
    public int TotalRepairCycleTime { get; set; }
    public int TotalRepairProductionCount { get; set; }

    [Column(TypeName = "decimal(12,6)")]
    public decimal TotalRepairSMV { get; set; }
    public string? BatchId { get; set; }
    public int? BatchSeq { get; set; }
    public string? PrinterId { get; set; }
    public string? Thumbnail { get; set; }
    public string? Content { get; set; }
    public DateTime? Completed { get; set; }
    public DateTime? Consumed { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal? QuantityUOM { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal? TotalScrapUOM { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal? TotalProductionStartedUOM { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal? TotalProductionFinishedUOM { get; set; }
    public int? TotalRepairCount { get; set; }
    public int? TotalReworkCount { get; set; }
    public int? RepairsToDo { get; set; }
    public int? ReworksToDo { get; set; }
    public string? Comments { get; set; }
    public string? UserName { get; set; }
    public int? TotalAwaitingCycleTime { get; set; }
}