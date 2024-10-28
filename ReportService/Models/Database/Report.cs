using System.ComponentModel.DataAnnotations;
using static ReportService.Models.Common.ReportCommon;

namespace ReportService.Models.Database
{
    public class Report
    {
        [Key]
        public Guid UUID { get; set; }
        public DateTime RequestDate { get; set; }
        public ReportStatus Status { get; set; }
        public string? Content { get; set; }
        public string MeterSerialNumber { get; set; }
    }
}
