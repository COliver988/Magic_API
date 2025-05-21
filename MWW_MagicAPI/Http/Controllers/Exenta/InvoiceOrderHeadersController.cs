using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWW_Api.Models.Exenta;
using MWW_Api.Repositories.Exenta;

namespace MWW_MagicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class InvoiceControllerHeadersController : ControllerBase
{
    private readonly IInvoiceOrderHeaderRepository _repo;

    public InvoiceOrderHeadersController(IInvoiceOrderHeaderRepository repo)
    {
        _repo = repo;
    }  

    [HttpGet("ByOrderNo")]
    public async Task<InvoiceOrderHeader?> GetByOrderNo(int orderNo)
    {
        return await _repo.GetByOrderNo(orderNo);
    }

    [HttpGet("InvoiceDetail")]
    public async Task<InvoiceOrderHeader?> GetInvoiceDetail(string legacy_vendor_id, string po, string companyCode = "MWW")
    {
        return await _repo.GetInvoiceDetail(legacy_vendor_id, po, companyCode);
    }
}
