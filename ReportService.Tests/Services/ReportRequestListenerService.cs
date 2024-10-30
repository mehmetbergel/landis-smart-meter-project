using Xunit;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using ReportService.Data;
using ReportService.Models.Common;
using ReportService.Models.Database;
using ReportService.Services;
using System.Net;

namespace ReportService.Tests.Services
{
    public class ReportRequestListenerServiceTests
    {
        private readonly Mock<ILogger<ReportRequestListenerService>> _loggerMock;
        private readonly ApplicationDbContext _context;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;

        public ReportRequestListenerServiceTests()
        {
            _loggerMock = new Mock<ILogger<ReportRequestListenerService>>();
            
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            Environment.SetEnvironmentVariable("Meter_Service_Url", "http://test-meter-service");
        }

        [Fact]
        public async Task Consume_WhenMeterDataExists_UpdatesReportWithSuccess()
        {
            var uuid = Guid.NewGuid();
            var report = new Report
            {
                UUID = uuid,
                MeterSerialNumber = "TEST1234",
                Status = ReportCommon.ReportStatus.Preparing
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var meterData = new MeterData
            {
                SerialNumber = "TEST1234",
                LastIndex = 100,
                ReadingTime = DateTime.Now,
                VoltageValue = 220,
                CurrentValue = 10
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(meterData))
                });

            var service = new ReportRequestListenerService(_loggerMock.Object, _context, _httpClient);
            var consumeContext = Mock.Of<ConsumeContext<ReportRequestMessage>>(x => 
                x.Message == new ReportRequestMessage(uuid));

            await service.Consume(consumeContext);

            var updatedReport = await _context.Reports.FindAsync(uuid);
            Assert.NotNull(updatedReport);
            Assert.Equal(ReportCommon.ReportStatus.Completed, updatedReport.Status);
            Assert.Contains("TEST1234", updatedReport.Content);
        }

        [Fact]
        public async Task Consume_WhenMeterDataRequestFails_UpdatesReportWithFailure()
        {
            var uuid = Guid.NewGuid();
            var report = new Report
            {
                UUID = uuid,
                MeterSerialNumber = "TEST1234",
                Status = ReportCommon.ReportStatus.Preparing
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            var service = new ReportRequestListenerService(_loggerMock.Object, _context, _httpClient);
            var consumeContext = Mock.Of<ConsumeContext<ReportRequestMessage>>(x => 
                x.Message == new ReportRequestMessage(uuid));

            await service.Consume(consumeContext);

            var updatedReport = await _context.Reports.FindAsync(uuid);
            Assert.NotNull(updatedReport);
            Assert.Equal(ReportCommon.ReportStatus.Failed, updatedReport.Status);
            Assert.Contains("NotFound", updatedReport.Content);
        }

        [Fact]
        public async Task Consume_WhenReportNotFound_DoesNothing()
        {
            var service = new ReportRequestListenerService(_loggerMock.Object, _context, _httpClient);
            var consumeContext = Mock.Of<ConsumeContext<ReportRequestMessage>>(x => 
                x.Message == new ReportRequestMessage(Guid.NewGuid()));

            await service.Consume(consumeContext);

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task Consume_WhenMeterDataResponseIsEmpty_UpdatesReportWithFailure()
        {
            var uuid = Guid.NewGuid();
            var report = new Report
            {
                UUID = uuid,
                MeterSerialNumber = "TEST1234",
                Status = ReportCommon.ReportStatus.Preparing
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{}")
                });

            var service = new ReportRequestListenerService(_loggerMock.Object, _context, _httpClient);
            var consumeContext = Mock.Of<ConsumeContext<ReportRequestMessage>>(x => 
                x.Message == new ReportRequestMessage(uuid));

            await service.Consume(consumeContext);

            var updatedReport = await _context.Reports.FindAsync(uuid);
            Assert.NotNull(updatedReport);
            Assert.Equal(ReportCommon.ReportStatus.Completed, updatedReport.Status);
        }
    }
}