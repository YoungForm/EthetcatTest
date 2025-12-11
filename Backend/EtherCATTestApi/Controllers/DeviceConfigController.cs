﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EtherCATTestApi.DataAccess.DbContext;
using EtherCATTestApi.DataAccess.Models;

namespace EtherCATTestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceConfigController : ControllerBase
    {
        private readonly EtherCATDbContext _context;

        public DeviceConfigController(EtherCATDbContext context)
        {
            _context = context;
        }

        // GET: api/DeviceConfig
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceConfig>>> GetDeviceConfigs()
        {
            return await _context.DeviceConfigs.ToListAsync();
        }

        // GET: api/DeviceConfig/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceConfig>> GetDeviceConfig(int id)
        {
            var deviceConfig = await _context.DeviceConfigs.FindAsync(id);

            if (deviceConfig == null)
            {
                return NotFound();
            }

            return deviceConfig;
        }

        // PUT: api/DeviceConfig/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDeviceConfig(int id, DeviceConfig deviceConfig)
        {
            if (id != deviceConfig.Id)
            {
                return BadRequest();
            }

            _context.Entry(deviceConfig).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceConfigExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/DeviceConfig
        [HttpPost]
        public async Task<ActionResult<DeviceConfig>> PostDeviceConfig(DeviceConfig deviceConfig)
        {
            _context.DeviceConfigs.Add(deviceConfig);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDeviceConfig", new { id = deviceConfig.Id }, deviceConfig);
        }

        // DELETE: api/DeviceConfig/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeviceConfig(int id)
        {
            var deviceConfig = await _context.DeviceConfigs.FindAsync(id);
            if (deviceConfig == null)
            {
                return NotFound();
            }

            _context.DeviceConfigs.Remove(deviceConfig);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DeviceConfigExists(int id)
        {
            return _context.DeviceConfigs.Any(e => e.Id == id);
        }
    }
}