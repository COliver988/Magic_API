using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MWW_MagicAPI.Http.Controllers.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : Controller
    {
        [HttpGet("Version")]
        [AllowAnonymous]
        public IActionResult Version()
        {
            // Get the assembly that contains this controller
            var assembly = typeof(SystemController).Assembly;

            // Get the version from the assembly's metadata
            Version version = assembly.GetName().Version;
            string frameworkDescription = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

            return Ok(new
            {
                AssemblyVersion = assembly.GetName().Version?.ToString() ?? "Unknown",
                CompileDate = ConvertDaysSinceEpochToDate(version != null && version.Build > 0 ? version.Build : 0).ToString("yyyy-MM-dd"),
                RunTime = frameworkDescription
            });
        }

        private DateTime ConvertDaysSinceEpochToDate(int daysSinceEpoch)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddDays(daysSinceEpoch);
        }
    }
}