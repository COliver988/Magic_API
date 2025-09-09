using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWW_Api.Models.Shopfloor;
using MWW_MagicAPI.Services;

namespace MWW_MagicAPI.Http.Controllers.Services
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ResetWorkOrder : Controller
    {
        private readonly IFixBatchService _fixBatchService;

        public ResetWorkOrder(IFixBatchService fixBatchService)
        {
            _fixBatchService = fixBatchService;
        }

        //NOTE: temp while testing 
        [HttpGet("FixBatch/{batchId}")]
        public async Task<IActionResult> FixBatch(string batchId)
        {
            List<Unit> units = await _fixBatchService.GetMissingBatches(batchId);
            return Ok(units);
        }
    }
}