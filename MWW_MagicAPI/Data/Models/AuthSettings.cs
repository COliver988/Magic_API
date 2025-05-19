namespace MWW_MagicAPI.Data.Models;

public class AuthSettings
{
    // provate key
    public string AuthKey { get; set; } = string.Empty;
    
    // minutes to be valid
    public int Timeout { get; set; } = 15;   
}