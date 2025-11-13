namespace MWW_Api.Models.Exenta;

public class Dimension
{
    public string? DIMENSION { get; set; }
    public string? DIMENSIONDESC { get; set; }
    public int Sc_iKey { get; set; }
    public DateTime Sc_TimeCreated { get; set; }
    public DateTime Sc_TimeLastMod { get; set; }
    public string? Sc_UserIdLastMod { get; set; }
    public string? COMPANYCODE { get; set; }
    public string? GREIGESTYLE { get; set; }
    public decimal DRSURCHARGE { get; set; }
    public decimal GREIGEFREIGHT { get; set; }
    public decimal GREIGEDESIGN { get; set; }
    public decimal WORKINGLOSSPERC { get; set; }
    public string? GREIGECOUNTRY { get; set; }
    public string? SIZERANGE { get; set; }
    public string? MERCHGROUPI { get; set; }
}
