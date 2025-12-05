using Microsoft.AspNetCore.Mvc;
using EtherCATTestApi.EtherCATTestEngine.ESI;
using System.IO;

namespace EtherCATTestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ESIController : ControllerBase
    {
        private readonly ESIParser _esiParser;
        private readonly ESIValidator _esiValidator;

        public ESIController(ESIParser esiParser, ESIValidator esiValidator)
        {
            _esiParser = esiParser;
            _esiValidator = esiValidator;
        }

        // POST: api/ESI/Parse
        [HttpPost("Parse")]
        public async Task<IActionResult> ParseESI(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            try
            {
                using var stream = file.OpenReadStream();
                var esiInfo = _esiParser.ParseESIStream(stream);
                return Ok(esiInfo);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to parse ESI file: {ex.Message}");
            }
        }

        // POST: api/ESI/Validate
        [HttpPost("Validate")]
        public async Task<IActionResult> ValidateESI(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            try
            {
                // Save file temporarily to validate
                var tempPath = Path.GetTempFileName() + ".xml";
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string validationErrors;
                var isValid = _esiValidator.ValidateESIFile(tempPath, out validationErrors);

                // Clean up temp file
                System.IO.File.Delete(tempPath);

                return Ok(new {
                    IsValid = isValid,
                    ValidationErrors = validationErrors
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to validate ESI file: {ex.Message}");
            }
        }

        // POST: api/ESI/ValidateConsistency
        [HttpPost("ValidateConsistency")]
        public async Task<IActionResult> ValidateConsistency(IFormFile file, [FromForm] ushort actualVendorId, [FromForm] uint actualProductCode)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            try
            {
                using var stream = file.OpenReadStream();
                var esiInfo = _esiParser.ParseESIStream(stream);
                
                var isConsistent = _esiValidator.ValidateDeviceInfoConsistency(esiInfo, actualVendorId, actualProductCode);
                
                return Ok(new {
                    IsConsistent = isConsistent,
                    ESIInfo = esiInfo,
                    ActualVendorId = actualVendorId,
                    ActualProductCode = actualProductCode
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to validate consistency: {ex.Message}");
            }
        }
    }
}