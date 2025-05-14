using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Magic;

[Table("MWW_APPLICATIONS")]
public class MWW_Applications
{
    [Key]
	public int recordID {get;set;}
	public string? RunningApps {get;set;}
	public DateTime? LastRunTime {get;set;}
	public decimal DelayInMinutes {get;set;}
	public string? HostServer {get;set;}
	public bool Active {get;set;}
	public string? Description {get;set;}
	public string? EXEName {get;set;}
	public string? SMSContact {get;set;}
	public string? HealthCheckURL {get;set;}
}
