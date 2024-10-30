using Xunit;
using Microsoft.EntityFrameworkCore;
using ReportService.Data;
using ReportService.Models.Common;
using ReportService.Models.Database;
using ReportService.Models.API;
using ReportService.Services;
using System.Text.Json;

namespace ReportService.Tests.Services
{
    public class ReportDownloadServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ReportDownloadService _service;

        public ReportDownloadServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _service = new ReportDownloadService(_context);
        }

        [Fact]
        public async Task GetData_ReturnsCorrectData()
        {
            var meterData = new MeterData
            {
                SerialNumber = "TEST1234",
                LastIndex = 100,
                ReadingTime = DateTime.Now,
                VoltageValue = 220,
                CurrentValue = 10
            };

            var report = new Report
            {
                UUID = Guid.NewGuid(),
                RequestDate = DateTime.Now,
                Status = ReportCommon.ReportStatus.Completed,
                MeterSerialNumber = "TEST1234",
                Content = JsonSerializer.Serialize(meterData)
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var result = await _service.GetData();

            Assert.Single(result);
            var firstReport = result[0][0];
            Assert.Equal(report.MeterSerialNumber, firstReport.MeterSerialNumber);
            Assert.Equal(meterData.LastIndex, firstReport.ContentDetail.LastIndex);
            Assert.Equal(meterData.VoltageValue, firstReport.ContentDetail.VoltageValue);
        }

        [Fact]
        public async Task GenerateExcel_ReturnsValidExcelFile()
        {
            var data = await SetupTestData();

            var excelBytes = _service.GenerateExcel(data);

            Assert.NotNull(excelBytes);
            Assert.True(excelBytes.Length > 0);
        }

        [Fact]
        public async Task GenerateCsv_ReturnsValidCsvFile()
        {
            var data = await SetupTestData();

            var csvBytes = _service.GenerateCsv(data);

            Assert.NotNull(csvBytes);
            Assert.True(csvBytes.Length > 0);
            var csvContent = System.Text.Encoding.UTF8.GetString(csvBytes);
            Assert.Contains("RequestDate,Status,MeterSerialNumber", csvContent);
        }

        [Fact]
        public async Task GetData_WithInvalidJsonContent_HandlesError()
        {
            var report = new Report
            {
                UUID = Guid.NewGuid(),
                RequestDate = DateTime.Now,
                Status = ReportCommon.ReportStatus.Completed,
                MeterSerialNumber = "TEST1234",
                Content = "invalid json"
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var result = await _service.GetData();

            Assert.Single(result);
            var firstReport = result[0][0];
            Assert.Equal(report.MeterSerialNumber, firstReport.MeterSerialNumber);
            Assert.Equal(0, firstReport.ContentDetail.LastIndex);
            Assert.Equal(0, firstReport.ContentDetail.VoltageValue);
        }

        [Fact]
        public async Task GenerateText_ReturnsValidTextFile()
        {
            var data = await SetupTestData();

            var textBytes = _service.GenerateText(data);

            Assert.NotNull(textBytes);
            Assert.True(textBytes.Length > 0);
            var textContent = System.Text.Encoding.UTF8.GetString(textBytes);
            Assert.Contains("RequestDate\tStatus\tMeterSerialNumber", textContent);
        }

        [Fact]
        public async Task GetData_WithEmptyDatabase_ReturnsEmptyList()
        {
            var result = await _service.GetData();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetData_WithNullContent_HandlesError()
        {
            var report = new Report
            {
                UUID = Guid.NewGuid(),
                RequestDate = DateTime.Now,
                Status = ReportCommon.ReportStatus.Completed,
                MeterSerialNumber = "TEST1234",
                Content = null
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var result = await _service.GetData();

            Assert.Single(result);
            var firstReport = result[0][0];
            Assert.Equal(report.MeterSerialNumber, firstReport.MeterSerialNumber);
            Assert.Equal(0, firstReport.ContentDetail.LastIndex);
        }

        [Fact]
        public async Task GenerateExcel_WithLargeDataset_HandlesCorrectly()
        {
            var reports = new List<Report>();
            for (int i = 0; i < 1000; i++)
            {
                var meterData = new MeterData
                {
                    SerialNumber = $"TEST{i}",
                    LastIndex = i,
                    ReadingTime = DateTime.Now,
                    VoltageValue = 220,
                    CurrentValue = 10
                };

                reports.Add(new Report
                {
                    UUID = Guid.NewGuid(),
                    RequestDate = DateTime.Now,
                    Status = ReportCommon.ReportStatus.Completed,
                    MeterSerialNumber = $"TEST{i}",
                    Content = JsonSerializer.Serialize(meterData)
                });
            }

            _context.Reports.AddRange(reports);
            await _context.SaveChangesAsync();

            var data = await _service.GetData();
            var excelBytes = _service.GenerateExcel(data);

            Assert.NotNull(excelBytes);
            Assert.True(excelBytes.Length > 0);
        }

        private async Task<List<ReportDownloadResponse[]>> SetupTestData()
        {
            return await _service.GetData();
        }
    }
}