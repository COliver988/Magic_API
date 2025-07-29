namespace MWW_MagicAPI.Data.Models.DTO;

public record OrdersByHourDTO
{
    public DateTime Date { get; set; }
    public int Hour { get; set; }
    public int Orders { get; set; }
    
    // JSON or Magic (maybe by user?)
    public string? Name { get; set; }
}