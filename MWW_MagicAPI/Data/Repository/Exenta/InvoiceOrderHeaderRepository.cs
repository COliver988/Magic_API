using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Exenta;

namespace MWW_Api.Repositories.Exenta;

public class InvoiceOrderHeaderRepository : IInvoiceOrderHeaderRepository
{
    private readonly ExentaDbContext _context;

    public InvoiceOrderHeaderRepository(ExentaDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceOrderHeader?> GetByOrderNo(int orderNo) => await _context.InvoiceOrderHeaders.Where(io => io.ORDERNO == orderNo).FirstOrDefaultAsync();

    public async Task<InvoiceOrderHeader?> GetInvoiceDetail(string legacy_vendor_id, string po, string companyCode = "MWW") =>
        await _context.InvoiceOrderHeaders.Where(io => io.CUSTOMER ==  legacy_vendor_id && io.ORDERREFERENCE == po && io.COMPANYCODE == companyCode).FirstOrDefaultAsync();
}
