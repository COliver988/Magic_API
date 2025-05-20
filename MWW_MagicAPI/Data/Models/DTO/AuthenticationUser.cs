namespace MWW_MagicAPI.Data.Models.DTO;

public record AuthenticationUser
{
    public string? Name { get; set; }
    public string? Secret { get; set; }
}
