namespace MWW_Api.Models.Shopfloor;

public class MileStone
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int CompanyId { get; set; }
    public string AlphaNumId { get; set; }
    public bool Active { get; set; }
    public string? GUID { get; set; }
    public DateTime? Modified { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime Created { get; set; }
    public int? DaysOffset { get; set; }
    public int SortOrder { get; set; }
    public int Kind { get; set; }
}
