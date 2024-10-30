using Xunit;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ReportService.Controllers;
using ReportService.Data;
using ReportService.Models.API;
using ReportService.Models.Common;
using ReportService.Models.Database;
using ReportService.Services;

namespace ReportService.Tests.Controllers
{
    public class ReportControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ISendEndpointProvider> _sendEndpointProviderMock;
        private readonly Mock<ReportDownloadService> _reportDownloadServiceMock;
        private readonly ReportController _controller;

        public ReportControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            
            _sendEndpointProviderMock = new Mock<ISendEndpointProvider>();
            _reportDownloadServiceMock = new Mock<ReportDownloadService>(_context);
            
            var sendEndpointMock = new Mock<ISendEndpoint>();
            _sendEndpointProviderMock
                .Setup(x => x.GetSendEndpoint(It.IsAny<Uri>()))
                .ReturnsAsync(sendEndpointMock.Object);

            _controller = new ReportController(_context, _sendEndpointProviderMock.Object, _reportDownloadServiceMock.Object);
        }

        [Fact]
        public async Task GetReports_ReturnsAllReports()
        {
            var report = new Report
            {
                UUID = Guid.NewGuid(),
                MeterSerialNumber = "TEST1234",
                Status = ReportCommon.ReportStatus.Completed
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var result = await _controller.GetReports();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Report>>>(result);
            var reports = Assert.IsAssignableFrom<IEnumerable<Report>>(actionResult.Value);
            Assert.Single(reports);
        }

        [Fact]
        public async Task PostReport_CreatesNewReport()
        {
            var request = new ReportCreateRequest { MeterSerialNumber = "TEST1234" };

            var result = await _controller.PostReport(request);

            var actionResult = Assert.IsType<ActionResult<Report>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var report = Assert.IsType<Report>(createdAtActionResult.Value);
            Assert.Equal(request.MeterSerialNumber, report.MeterSerialNumber);
            Assert.Equal(ReportCommon.ReportStatus.Preparing, report.Status);
        }

        [Fact]
        public async Task Download_ReturnsCorrectFileType()
        {
            var testData = new List<ReportDownloadResponse[]>();
            _reportDownloadServiceMock
                .Setup(x => x.GetData())
                .ReturnsAsync(testData);
            _reportDownloadServiceMock
                .Setup(x => x.GenerateExcel(It.IsAny<List<ReportDownloadResponse[]>>()))
                .Returns(new byte[] { 1, 2, 3 });

            var result = await _controller.Download("excel");

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Contains(".xlsx", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task GetReport_WithValidId_ReturnsReport()
        {
            var report = new Report
            {
                UUID = Guid.NewGuid(),
                MeterSerialNumber = "TEST1234",
                Status = ReportCommon.ReportStatus.Completed
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var result = await _controller.GetReport(report.UUID);

            var actionResult = Assert.IsType<ActionResult<Report>>(result);
            var returnedReport = Assert.IsType<Report>(actionResult.Value);
            Assert.Equal(report.UUID, returnedReport.UUID);
        }

        [Fact]
        public async Task GetReport_WithInvalidId_ReturnsNotFound()
        {
            var result = await _controller.GetReport(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PutReport_WithValidData_UpdatesReport()
        {
            var report = new Report
            {
                UUID = Guid.NewGuid(),
                MeterSerialNumber = "TEST1234",
                Status = ReportCommon.ReportStatus.Completed
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            report.Status = ReportCommon.ReportStatus.Failed;

            var result = await _controller.PutReport(report.UUID, report);

            Assert.IsType<NoContentResult>(result);
            var updatedReport = await _context.Reports.FindAsync(report.UUID);
            Assert.Equal(ReportCommon.ReportStatus.Failed, updatedReport.Status);
        }

        [Fact]
        public async Task DeleteReport_WithValidId_RemovesReport()
        {
            var report = new Report
            {
                UUID = Guid.NewGuid(),
                MeterSerialNumber = "TEST1234",
                Status = ReportCommon.ReportStatus.Completed
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteReport(report.UUID);

            Assert.IsType<NoContentResult>(result);
            Assert.Null(await _context.Reports.FindAsync(report.UUID));
        }

        [Fact]
        public async Task Download_WithInvalidFileType_ReturnsBadRequest()
        {
            var result = await _controller.Download("invalid");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task PutReport_WithMismatchedIds_ReturnsBadRequest()
        {
            var report = new Report
            {
                UUID = Guid.NewGuid(),
                MeterSerialNumber = "TEST1234",
                Status = ReportCommon.ReportStatus.Completed
            };

            var result = await _controller.PutReport(Guid.NewGuid(), report);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PutReport_WithNonExistentReport_ReturnsNotFound()
        {
            var report = new Report
            {
                UUID = Guid.NewGuid(),
                MeterSerialNumber = "TEST1234",
                Status = ReportCommon.ReportStatus.Completed
            };

            var result = await _controller.PutReport(report.UUID, report);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteReport_WithNonExistentId_ReturnsNotFound()
        {
            var result = await _controller.DeleteReport(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData("csv", "text/csv")]
        [InlineData("txt", "text/plain")]
        public async Task Download_WithDifferentFileTypes_ReturnsCorrectContentType(string fileType, string expectedContentType)
        {
            var testData = new List<ReportDownloadResponse[]>();
            _reportDownloadServiceMock
                .Setup(x => x.GetData())
                .ReturnsAsync(testData);
            _reportDownloadServiceMock
                .Setup(x => x.GenerateCsv(It.IsAny<List<ReportDownloadResponse[]>>()))
                .Returns(new byte[] { 1, 2, 3 });
            _reportDownloadServiceMock
                .Setup(x => x.GenerateText(It.IsAny<List<ReportDownloadResponse[]>>()))
                .Returns(new byte[] { 1, 2, 3 });

            var result = await _controller.Download(fileType);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(expectedContentType, fileResult.ContentType);
        }
    }
}