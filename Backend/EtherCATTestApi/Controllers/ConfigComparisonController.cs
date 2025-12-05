using System;using Microsoft.AspNetCore.Mvc;using EtherCATTestApi.EtherCATTestEngine.DiffTool;using EtherCATTestApi.EtherCATTestEngine.ESI;using Serilog;

namespace EtherCATTestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigComparisonController : ControllerBase
    {
        private readonly ConfigComparator _configComparator;
        private readonly ESIValidator _esiValidator;

        public ConfigComparisonController(
            ConfigComparator configComparator,
            ESIValidator esiValidator)
        {
            _configComparator = configComparator;
            _esiValidator = esiValidator;
        }

        /// <summary>
        /// Compare ESI file configuration with actual device configuration
        /// </summary>
        /// <param name="request">Comparison request containing ESI file path, vendor ID, and product code</param>
        /// <returns>Configuration comparison results</returns>
        [HttpPost("compare")]
        public async Task<ActionResult<ConfigDiffResult>> CompareConfig([FromBody] ConfigComparisonRequest request)
        {
            try
            {
                Log.Information("Starting configuration comparison");

                // Validate ESI file first
                var esiValidationResult = await _esiValidator.ValidateESIFileAsync(request.EsiFilePath);
                if (!esiValidationResult.IsValid)
                {
                    Log.Error("ESI validation failed: {Errors}", string.Join(", ", esiValidationResult.Errors));
                    return BadRequest(new { Message = "Invalid ESI file", Errors = esiValidationResult.Errors });
                }

                // Parse ESI file to get configuration info
                var esiInfo = await _esiValidator.ParseESIFileAsync(request.EsiFilePath);

                // Compare with actual device configuration
                var comparisonResult = await _configComparator.CompareConfig(
                    esiInfo,
                    request.ActualVendorId,
                    request.Act