using static ReportService.Models.Common.ReportCommon;

namespace ReportService.Models.API
{
    public class ReportDownloadResponse
    {
        public Guid UUID { get; set; }
        public DateTime RequestDate { get; set; }
        public ReportStatus Status { get; set; }
        public ContentDetail ContentDetail { get; set; }
        public string MeterSerialNumber { get; set; }
    }

    public class ContentDetail
    {
        public decimal LastIndex { get; set; }
        public DateTime ReadingTime { get; set; }
        public string SerialNumber { get; set; }
        public decimal VoltageValue { get; set; }
        public decimal CurrentValue { get; set; }

    }
}
