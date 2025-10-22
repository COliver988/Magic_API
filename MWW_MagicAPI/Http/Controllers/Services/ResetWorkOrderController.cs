using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWW_MagicAPI.Data.Models.DTO;
using MWW_MagicAPI.Services;

namespace MWW_MagicAPI.Http.Controllers.Services
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResetWorkOrderController : Controller
    {
        private readonly IFixBatchService _fixBatchService;

        public ResetWorkOrderController(IFixBatchService fixBatchService)
        {
            _fixBatchService = fixBatchService;
        }

        //NOTE: temp while testing 
        [HttpGet("FixBatch/{batchId}")]
        public async Task<IActionResult> FixBatch(string batchId)
        {
            List<WorkOrderDataDTO> units = await _fixBatchService.GetMissingBatches(batchId);
            return Ok(units);
        }
    }
}