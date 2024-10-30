using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportService.Data;
using ReportService.Models.API;
using ReportService.Models.Database;
using ReportService.Services;

namespace ReportService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ReportDownloadService _reportDownloadService;

        public ReportController(ApplicationDbContext context, ISendEndpointProvider sendEndpointProvider, ReportDownloadService reportDownloadService)
        {
            _context = context;
            _sendEndpointProvider = sendEndpointProvider;
            _reportDownloadService = reportDownloadService;
        }

        // GET: api/Report
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetReports()
        {
            return await _context.Reports.ToListAsync();
        }

        // GET: api/Report/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetReport(Guid id)
        {
            var report = await _context.Reports.FindAsync(id);

            if (report == null)
            {
                return NotFound();
            }

            return report;
        }


        [HttpGet("download/{fileType}")]
        public async Task<IActionResult> Download(string fileType)
        {
            byte[] fileBytes;
            string contentType;
            string fileName;
            var data = await _reportDownloadService.GetData();

            switch (fileType.ToLower())
            {
                case "excel":
                    fileBytes = _reportDownloadService.GenerateExcel(data);
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileName = $"rapor_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    break;

                case "csv":
                    fileBytes = _reportDownloadService.GenerateCsv(data);
                    contentType = "text/csv";
                    fileName = $"rapor_{DateTime.Now:yyyyMMddHHmmss}.csv";
                    break;

                case "txt":
                    fileBytes = _reportDownloadService.GenerateText(data);
                    contentType = "text/plain";
                    fileName = $"rapor_{DateTime.Now:yyyyMMddHHmmss}.txt";
                    break;

                default:
                    return BadRequest(new { message = "Geçersiz dosya tipi" });
            }

            return File(fileBytes, contentType, fileName);
        }

        // PUT: api/Report/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReport(Guid id, Report report)
        {
            if (id != report.UUID)
            {
                return BadRequest();
            }

            _context.Entry(report).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportExists(id))
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

        // POST: api/Report
        [HttpPost]
        public async Task<ActionResult<Report>> PostReport(ReportCreateRequest request)
        {
            var report = new Report
            {
                UUID = Guid.NewGuid(),
                RequestDate = DateTime.Now,
                Status = Models.Common.ReportCommon.ReportStatus.Preparing,
                MeterSerialNumber = request.MeterSerialNumber
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();


            var message = new ReportRequestMessage(report.UUID);
            var endPoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:report-queue"));
            await endPoint.Send(message);

            return CreatedAtAction("GetReport", new { id = report.UUID }, report);
        }

        // DELETE: api/Report/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(Guid id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReportExists(Guid id)
        {
            return _context.Reports.Any(e => e.UUID == id);
        }
    }
}
