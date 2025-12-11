﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EtherCATTestApi.DataAccess.DbContext;
using EtherCATTestApi.DataAccess.Models;

namespace EtherCATTestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestResultController : ControllerBase
    {
        private readonly EtherCATDbContext _context;

        public TestResultController(EtherCATDbContext context)
        {
            _context = context;
        }

        // GET: api/TestResult
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestResult>>> GetTestResults()
        {
            return await _context.TestResults.Include(tr => tr.TestSession).ToListAsync();
        }

        // GET: api/TestResult/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestResult>> GetTestResult(int id)
        {
            var testResult = await _context.TestResults.Include(tr => tr.TestSession).FirstOrDefaultAsync(tr => tr.Id == id);

            if (testResult == null)
            {
                return NotFound();
            }

            return testResult;
        }

        // GET: api/TestResult/Session/5
        [HttpGet("Session/{sessionId}")]
        public async Task<ActionResult<IEnumerable<TestResult>>> GetTestResultsBySessionId(int sessionId)
        {
            return await _context.TestResults.Where(tr => tr.TestSessionId == sessionId).ToListAsync();
        }

        // PUT: api/TestResult/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTestResult(int id, TestResult testResult)
        {
            if (id != testResult.Id)
            {
                return BadRequest();
            }

            _context.Entry(testResult).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TestResultExists(id))
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

        // POST: api/TestResult
        [HttpPost]
        public async Task<ActionResult<TestResult>> PostTestResult(TestResult testResult)
        {
            _context.TestResults.Add(testResult);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTestResult", new { id = testResult.Id }, testResult);
        }

        // DELETE: api/TestResult/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestResult(int id)
        {
            var testResult = await _context.TestResults.FindAsync(id);
            if (testResult == null)
            {
                return NotFound();
            }

            _context.TestResults.Remove(testResult);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TestResultExists(int id)
        {
            return _context.TestResults.Any(e => e.Id == id);
        }
    }
}