using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeterService.Data;
using MeterService.Models;
using MeterService.Services;

namespace MeterService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeterController : ControllerBase
    {
        private readonly MeterReadingService _meterReadingService;
        private readonly ApplicationDbContext _context;

        public MeterController(ApplicationDbContext context, MeterReadingService meterReadingService)
        {
            _context = context;
            _meterReadingService = meterReadingService;
        }

        // GET: api/Meter
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MeterReading>>> GetMeterReadings()
        {
            return await _context.MeterReadings
                 .OrderByDescending(m => m.ReadingTime)
                 .ToListAsync();
        }

        // GET: api/Meter/5
        [HttpGet("{id}")]
        public ActionResult<MeterReading> GetMeterReading(string id)
        {
            var meterReading = _context.MeterReadings.Where(x => x.SerialNumber == id).FirstOrDefault();

            if (meterReading == null)
            {
                return NotFound();
            }

            return meterReading;
        }

        // PUT: api/Meter/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMeterReading(Guid id, MeterReading meterReading)
        {
            if (id != meterReading.UUID)
            {
                return BadRequest();
            }

            _context.Entry(meterReading).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MeterReadingExists(id))
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

        // POST: api/Meter
        [HttpPost]
        public async Task<ActionResult<MeterReading>> PostMeterReading(MeterReading meterReading)
        {
            var meter = _meterReadingService.CreateMeterReading(meterReading);
            _context.MeterReadings.Add(meter);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMeterReading", new { id = meterReading.UUID }, meterReading);
        }

        // DELETE: api/Meter/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeterReading(Guid id)
        {
            var meterReading = await _context.MeterReadings.FindAsync(id);
            if (meterReading == null)
            {
                return NotFound();
            }

            _context.MeterReadings.Remove(meterReading);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MeterReadingExists(Guid id)
        {
            return _context.MeterReadings.Any(e => e.UUID == id);
        }
    }
}
