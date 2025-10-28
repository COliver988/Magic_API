using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Magic;

[Table("WEB_Items")]
public class WebItem
{
    public string Item_code { get; set; }
    public string? MWWItemCode { get; set; }
    public string? Item_desc { get; set; }
    public string? Item_type { get; set; }
    public string? DAPFrameType { get; set; }
    public string? DAPABS { get; set; }
    public string? Attr_FrameColor { get; set; }
    public string? Active { get; set; }
    public string? LicenseDesc { get; set; }
    public string? ART_NUM { get; set; }
    public string? Design { get; set; }
    public string? Product_Type { get; set; }
    public string? LCS { get; set; }
    public string? ImportServers { get; set; }
}
