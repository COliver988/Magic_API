using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Magic;

public class DapPartner
{
    [Key]
    public decimal UNIQ_ID { get; set; }
    public string? PO { get; set; }
    public string? BILL_NAME { get; set; }
    public string? BILL_ADDRESS1 { get; set; }
    public string? BILL_ADDRESS2 { get; set; }
    public string? BILL_CITY { get; set; }
    public string? BILL_STATE { get; set; }
    public string? BILL_ZIP { get; set; }
    public string? BILL_PHONE { get; set; }
    public string? EMAIL { get; set; }
    public string? SHIP_NAME { get; set; }
    public string? SHIP_ADDRESS1 { get; set; }
    public string? SHIP_ADDRESS2 { get; set; }
    public string? SHIP_CITY { get; set; }
    public string? SHIP_STATE { get; set; }
    public string? SHIP_ZIP { get; set; }
    public string? SHIP_INTL_STATE { get; set; }
    public string? SHIP_COUNTRY { get; set; }
    public string? SHIP_PHONE { get; set; }
    public string? CANVAS { get; set; }
    public string? GALLERY_WRAP { get; set; }
    public string? FRAME_COLOR { get; set; }
    public string? BRUSH_STROKES { get; set; }
    public string? RETOUCH { get; set; }
    public string? NOTES { get; set; }
    public string? IMG_FILENAME { get; set; }

    [Column(TypeName = "decimal(19,4)")]
    public decimal? ITEM_TOTAL { get; set; }

    [Column(TypeName = "decimal(19,4)")]
    public decimal? SHIPPING_TOTAL { get; set; }

    [Column(TypeName = "decimal(19,4)")]
    public decimal? GRAND_TOTAL { get; set; }
    public DateTime DATE_PLACED { get; set; }
    public string? CC_NAME { get; set; }
    public string? CC_TYPE { get; set; }
    public string? CC_NUM { get; set; }
    public string? CC_EXP_MON { get; set; }
    public string? CC_EXP_YEAR { get; set; }
    public string? CC_CCV { get; set; }
    public string? CC_APPROVED { get; set; }
    public string? PROC_FS { get; set; }
    public string? PROC_DB { get; set; }
    public string? CTA { get; set; }
    public string? CANVAS_DESC { get; set; }
    public string? GALLERY_WRAP_DESC { get; set; }
    public string? FRAME_DESC { get; set; }
    public string? BRUSH_STROKES_DESC { get; set; }
    public string? RETOUCH_DESC { get; set; }
    public string? SHIPPING_METHOD { get; set; }
    public string? CANVAS_BASE_PRICE { get; set; }
    public string? GALLERY_WRAP_PRICE { get; set; }
    public string? BRUSH_STROKES_PRICE { get; set; }

    [Column(TypeName = "decimal(19,4)")]
    public decimal? RETOUCH_PRICE { get; set; }

    [Column(TypeName = "decimal(19,4)")]
    public decimal? TAX { get; set; }
    public string? PROMOTION_NUM { get; set; }

    [Column(TypeName = "decimal(19,4)")]
    public decimal? PROMOTION_AMT { get; set; }
    public string? TAPESTRY_COLOR { get; set; }
    public string? SIGNATURE { get; set; }
    public string? KEY_CODE { get; set; }
    public string? WM_PO { get; set; }
    public string? CUST_ID { get; set; }
    public string? EMAIL_ORDERBY { get; set; }
    public decimal? SHIPPING_SURCHARGE { get; set; }
    public decimal? SHIPPING_BASE_CHG { get; set; }
    public string? SHIPPING_SURCHARGE_ITEM { get; set; }
    public int? Quantity { get; set; }
    public string? CO_NUMBER { get; set; }
    public string? REF_URL { get; set; }
    public string? ENVELOPE { get; set; }
    public string? REFERENCE { get; set; }
    public string? BROWSER_INFO { get; set; }
    public int? STATUS { get; set; }
    public string? SHIP_ID { get; set; }
    public DateTime? NewOrderEmailSent { get; set; }
    public string? WIPEmailSent { get; set; }
    public DateTime? ShipNotificationEmailSent { get; set; }
    public string? TKStoreRefNum { get; set; }
    public string? TKRef1 { get; set; }
    public string? TKRef2 { get; set; }
    public string? TKCName { get; set; }
    public string? TKCPhone { get; set; }
    public string? TKImageMove { get; set; }
    public string? WMDBkgType { get; set; }
    public string? WMDTemplateName { get; set; }
    public string? WMDHeadText { get; set; }
    public string? WMDQuoteText { get; set; }
    public string? WMDPersonalText { get; set; }
    public string? EMVRESULTS { get; set; }
    public string? CC_CHARGED { get; set; }
    public string? FS_IMG_Move_Path { get; set; }
    public string? FS_Status { get; set; }
    public string? FS_StatusDescription { get; set; }
    public DateTime? FS_StatusDate { get; set; }
    public string? FS_ShipCompany { get; set; }
    public decimal? FS_ShipWeight { get; set; }
    public decimal? FS_ShipCost { get; set; }
    public string? FS_TrackingNumber { get; set; }
    public string? FS_Group { get; set; }
    public string? FS_CertField { get; set; }
    public string? FS_ComCode { get; set; }
    public string? FS_LN { get; set; }
    public string? FS_Item { get; set; }
    public string? FS_ShipQty { get; set; }
    public string? FS_PickerShipperID { get; set; }
    public DateTime? FS_ShipDate { get; set; }
    public string? Specification { get; set; }
    public DateTime? Date_Entered { get; set; }
    public string? Has_Postmark { get; set; }
    public DateTime? Postmark_Date { get; set; }
    public int? FJOrder_ID { get; set; }
    public int? FJLabID { get; set; }
    public string? TKDealerName { get; set; }
    public string? TKPOTrunc { get; set; }
    public string? TKDistribStoreCode { get; set; }
    public string? TKRetailerSkuCode { get; set; }
    public string? TKUPCCode { get; set; }
    public string? TKRetailerProductName { get; set; }
    public string? TKRetailerMSRP { get; set; }
    public string? LBL_FILENAME { get; set; }
    public string? Artist { get; set; }
    public DateTime? Art_AssignDate { get; set; }
    public DateTime? Art_CompleteDate { get; set; }
    public string? receipt_filename { get; set; }
    public string? TKRouteCode { get; set; }
    public string? LockedBy { get; set; }
    public string? TKPaymentStatus { get; set; }
    public string? Background_Color { get; set; }
    public string? Letter_Color { get; set; }
    public string? Font_Style { get; set; }
    public string? multi_order_info { get; set; }
    public DateTime? modifydate { get; set; }
    public string? MONumber { get; set; }
    public string? MOStartDate { get; set; }
    public string? MOSchedDate { get; set; }
    public string? CO_Prom_Del { get; set; }
    public int? CO_LN { get; set; }
    public DateTime? MOCreated { get; set; }
    public string? TicketGenerated { get; set; }
    public decimal? EDI_STATUS { get; set; }
    public string? ImportServers { get; set; }
    public DateTime? CREATED_AT { get; set; }
    public string? PrintLocation { get; set; }
    public int? IsValidAddr { get; set; }
    public string? Marketplace { get; set; }
}
