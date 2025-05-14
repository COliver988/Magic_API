using  MWW_Api.Models.Exenta;

namespace MWW_Api.Repositories.Exenta;

public interface IInvoiceOrderHeaderRepository
{
    public Task<InvoiceOrderHeader?> GetByOrderNo(int orderNo);
    public Task<InvoiceOrderHeader?> GetInvoiceDetail(string legacy_vendor_id, string po, string companyCode);
}
