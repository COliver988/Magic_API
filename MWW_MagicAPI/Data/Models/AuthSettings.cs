namespace MWW_MagicAPI.Data.Models;

public class AuthSettings
{
    // private key
    public string PrivateKey { get; set; }
    
    // minutes to be valid
    public int Timeout { get; set; }
    public string Audience { get; set; }
    public string Issuer { get; set; }
}