using static ReportService.Models.Common.ReportCommon;

namespace ReportService.Models.API
{
    public class ReportCreateResponse
    {
        public Guid UUID { get; set; }
        public ReportStatus Status { get; set; }

    }
}
