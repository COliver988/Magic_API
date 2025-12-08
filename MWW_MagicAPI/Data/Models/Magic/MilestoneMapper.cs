namespace MWW_Api.Models.Magic;

public class MilestoneMapper
{
    [Column("id")]
    public int Id {get;set;}

    [Column("milestone")]
	public string Milestone {get;set;}

    [Column("new_status")]
	public string NewStatus {get;set;}
 
    [Column("fs_status")]
	public string? FS_Status {get;set;}

}