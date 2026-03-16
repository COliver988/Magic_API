using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Peeps;

[Table("hangfirejobs")]
public class HangfireJob
{
    [Column("id")]
    [Key]
    public int Id { get; set; }
    
    [Column("job_id")]
    public required string JobId { get; set; }
    
    [Column("queue")]
    public required string Queue { get; set; }

    [Column("server")]
    public required string Server { get; set; }

    [Column("service_type_name")]
    public required string ServiceTypeName { get; set; }

    [Column("cron_expression")]
    public required string CronExpression { get; set; }

    [Column("parameters")]
    public string? Parameters { get; set; }

    [Column("job_name")]
    public required string JobName { get; set; }

    [Column("is_active")]
    public required bool IsActive { get; set; }
}