using Microsoft.AspNetCore.Mvc;
using EtherCATTestApi.EtherCATTestEngine.DiffTool;
using EtherCATTestApi.EtherCATTestEngine.ESI;
using Serilog;
using System.Threading.Tasks;
using System.IO;

namespace EtherCATTestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigComparisonController : ControllerBase
    {
        private readonly ConfigComparator _configComparator;
        private readonly ESIValidator _esiValidator;
        private readonly ESIParser _esiParser;

        public ConfigComparisonController(
            ConfigComparator configComparator,
            ESIValidator esiValidator,
            ESIParser esiParser)
        {
            _configComparator = configComparator;
            _esiValidator = esiValidator;
            _esiParser = esiParser;
        }

        [HttpPost("compare")]
        public async Task<ActionResult<ConfigDiffResult>> Compare([FromForm] IFormFile file, [FromForm] ushort actualVendorId, [FromForm] uint actualProductCode)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { Message = "No file uploaded" });
                }

                var tempPath = Path.GetTempFileName() + ".xml";
                using (var fs = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                string validationErrors;
                var isValid = _esiValidator.ValidateESIFile(tempPath, out validationErrors);
                if (!isValid)
                {
                    System.IO.File.Delete(tempPath);
                    return BadRequest(new { Message = "Invalid ESI file", Errors = validationErrors });
                }

                using var stream = new FileStream(tempPath, FileMode.Open, FileAccess.Read);
                var esiInfo = _esiParser.ParseESIStream(stream);
                System.IO.File.Delete(tempPath);

                var comparisonResult = await _configComparator.CompareConfig(
                    esiInfo,
                    actualVendorId,
                    actualProductCode);

                return Ok(comparisonResult);
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "Configuration comparison failed");
                return BadRequest(new { Message = "Comparison failed", Error = ex.Message });
            }
        }
    }
}
