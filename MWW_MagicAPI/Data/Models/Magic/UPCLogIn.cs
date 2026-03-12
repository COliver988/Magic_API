using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MWW_Api.Models.Magic;


public class UPCLogIn
{
    public string? CO_NUMBER {get;set;}
	public string? CUST_ID {get;set;}
	public string? CUST_NAME {get;set;}
	public string? SHIP_VIA {get;set;}
	public string? CUST_PO_NO {get;set;}
	public string? LOG_DATE {get;set;}
	public string? USERID {get;set;}
	public string? PRINT_DATE {get;set;}
	public string? REQ_DATE {get;set;}
	public DateTime? CreateDate {get;set;}

    [Key]
	[Column(TypeName = "decimal(18, 0)")]
	 [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long RecordID {get;set;}
	public string? TrackNotes {get;set;}
	public string? BADGE_ID {get;set;}
	public string? SYSTEM_NAME {get;set;}        
}