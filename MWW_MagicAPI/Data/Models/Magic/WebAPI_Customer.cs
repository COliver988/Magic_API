namespace MWW_Api.Models.Magic;

public class WebAPI_Customer
{
    public string CustID { get; set; }
    public string VenderName { get; set; }
    public string? VenderPassword { get; set; }
    public string? Email { get; set; }
    public string? CC { get; set; }
    public string? BCC { get; set; }
    public string? ReplyTo { get; set; }
    public string? FTPPath { get; set; }
    public bool? NeedShippingInfo { get; set; }
    public bool? Active { get; set; }
    public string? DailyTracking { get; set; }
    public string? Status30Day { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public string? CustName { get; set; }
    public string? POPrefix { get; set; }
    public string? POResetDate { get; set; }
    public int? POCounter { get; set; }
    public int? PODefaultCounter { get; set; }
    public string? Subject { get; set; }
    public string? PriceBook { get; set; }
    public string? ImageErrorCC { get; set; }
    public bool? EnableReports { get; set; }
    public int? OrgImagesInDays { get; set; }
    public string? ImportServers { get; set; }
    public string? Terms { get; set; }
    public string? SalesMan { get; set; }
    public string? PeepsAPIKey { get; set; }
    public bool? EnableAddrVrify { get; set; }
    public bool? IsTkref2Exenta { get; set; }
    public bool? DisableLabelDownloadVen { get; set; }
    public string? Origin { get; set; }
    public string? MonthlyReportDay { get; set; }
    public int? Discount { get; set; }
    public string? DefaultLocation { get; set; }
    public string? PackingSlipFormat { get; set; }
    public int? SendReportEvery { get; set; }
}