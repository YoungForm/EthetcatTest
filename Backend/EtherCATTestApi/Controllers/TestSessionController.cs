﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EtherCATTestApi.DataAccess.DbContext;
using EtherCATTestApi.DataAccess.Models;

namespace EtherCATTestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestSessionController : ControllerBase
    {
        private readonly EtherCATDbContext _context;

        public TestSessionController(EtherCATDbContext context)
        {
            _context = context;
        }

        // GET: api/TestSession
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestSession>>> GetTestSessions()
        {
            return await _context.TestSessions.Include(ts => ts.DeviceConfig).ToListAsync();
        }

        // GET: api/TestSession/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestSession>> GetTestSession(int id)
        {
            var testSession = await _context.TestSessions.Include(ts => ts.DeviceConfig)
                                                      .Include(ts => ts.TestResults)
                                                      .FirstOrDefaultAsync(ts => ts.Id == id);

            if (testSession == null)
            {
                return NotFound();
            }

            return testSession;
        }

        // PUT: api/TestSession/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTestSession(int id, TestSession testSession)
        {
            if (id != testSession.Id)
            {
                return BadRequest();
            }

            _context.Entry(testSession).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TestSessionExists(id))
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

        // POST: api/TestSession
        [HttpPost]
        public async Task<ActionResult<TestSession>> PostTestSession(TestSession testSession)
        {
            _context.TestSessions.Add(testSession);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTestSession", new { id = testSession.Id }, testSession);
        }

        // DELETE: api/TestSession/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestSession(int id)
        {
            var testSession = await _context.TestSessions.FindAsync(id);
            if (testSession == null)
            {
                return NotFound();
            }

            _context.TestSessions.Remove(testSession);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TestSessionExists(int id)
        {
            return _context.TestSessions.Any(e => e.Id == id);
        }
    }
}