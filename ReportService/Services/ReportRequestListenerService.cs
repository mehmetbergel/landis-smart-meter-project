using MassTransit;
using Microsoft.EntityFrameworkCore;
using ReportService.Data;
using ReportService.Models.Database;
using System.Text.Json;

namespace ReportService.Services
{
    public class MeterData
    {
        public string SerialNumber { get; set; }
        public DateTime ReadingTime { get; set; }
        public decimal LastIndex { get; set; }
        public decimal VoltageValue { get; set; }
        public decimal CurrentValue { get; set; }
    }

    public record ReportRequestMessage(Guid UUID);
    public class ReportRequestListenerService : IConsumer<ReportRequestMessage>
    {
        private readonly ILogger<ReportRequestListenerService> logger;
        private readonly ApplicationDbContext applicationDbContext;
        private readonly HttpClient httpClient;

        public ReportRequestListenerService(ILogger<ReportRequestListenerService> logger, ApplicationDbContext applicationDbContext, HttpClient httpClient)
        {
            this.logger = logger;
            this.applicationDbContext = applicationDbContext;
            this.httpClient = httpClient;
            this.httpClient.BaseAddress = new Uri("https://localhost:7126");
        }

        public async Task Consume(ConsumeContext<ReportRequestMessage> context)
        {
            logger.LogInformation(context.Message.UUID.ToString());
            var createdReport = applicationDbContext.Reports.Where(x => x.UUID == context.Message.UUID).FirstOrDefault();

            if (createdReport != null)
            {
                var meterSerialNumber = createdReport?.MeterSerialNumber;
                var response = await httpClient.GetAsync($"api/Meter/{meterSerialNumber}");
                if (response.IsSuccessStatusCode)
                {
                    var meterData = await response.Content.ReadFromJsonAsync<MeterData>();
                    createdReport.Content = JsonSerializer.Serialize(meterData);
                    createdReport.Status = Models.Common.ReportCommon.ReportStatus.Completed;
                }
                else
                {
                    logger.LogError($"Meter data alınamadı. Status code: {response.StatusCode}");
                    createdReport.Status = Models.Common.ReportCommon.ReportStatus.Failed;
                    createdReport.Content = $"Meter verisi alınamadı: {response.StatusCode}";
                }

                applicationDbContext.Entry(createdReport).State = EntityState.Modified;
                await applicationDbContext.SaveChangesAsync();
            }
        }
    }
}
