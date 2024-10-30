using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ReportService.Data;
using ReportService.Models.API;

namespace ReportService.Services
{
    public class ReportDownloadService
    {
        private readonly ApplicationDbContext _context;

        public ReportDownloadService(ApplicationDbContext context)
        {
            _context = context;
        }

        private ContentDetail ParseContentDetail(string jsonContent)
        {
            try
            {
                var jsonDocument = JsonDocument.Parse(jsonContent);
                var root = jsonDocument.RootElement;

                return new ContentDetail
                {
                    LastIndex = root.GetProperty("LastIndex").GetDecimal(),
                    ReadingTime = root.GetProperty("ReadingTime").GetDateTime(),
                    SerialNumber = root.GetProperty("SerialNumber").GetString(),
                    VoltageValue = root.GetProperty("VoltageValue").GetDecimal(),
                    CurrentValue = root.GetProperty("CurrentValue").GetDecimal()
                };
            }
            catch (Exception ex)
            {
                return new ContentDetail();
            }
        }

        public virtual async Task<List<ReportDownloadResponse[]>> GetData()
        {
            var reports = await _context.Reports.ToListAsync();
            var result = new List<ReportDownloadResponse[]>();

            foreach (var report in reports)
            {
                var contentDetail = ParseContentDetail(report.Content);

                result.Add(new ReportDownloadResponse[]
                {
                    new ReportDownloadResponse
                    {
                        RequestDate = report.RequestDate,
                        Status = report.Status,
                        ContentDetail = contentDetail,
                        MeterSerialNumber = report.MeterSerialNumber
                    }
                });
            }

            return result;
        }

        public virtual byte[] GenerateExcel(List<ReportDownloadResponse[]> data)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Rapor");

                // Başlıkları ekle
                worksheet.Cells[1, 1].Value = "RequestDate";
                worksheet.Cells[1, 2].Value = "Status";
                worksheet.Cells[1, 3].Value = "MeterSerialNumber";
                worksheet.Cells[1, 4].Value = "LastIndex";
                worksheet.Cells[1, 5].Value = "ReadingTime";
                worksheet.Cells[1, 6].Value = "SerialNumber";
                worksheet.Cells[1, 7].Value = "VoltageValue";
                worksheet.Cells[1, 8].Value = "CurrentValue";

                // Verileri ekle
                int row = 2;
                foreach (var reportArray in data)
                {
                    foreach (var report in reportArray)
                    {
                        worksheet.Cells[row, 1].Value = report.RequestDate;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                        worksheet.Cells[row, 2].Value = report.Status;
                        worksheet.Cells[row, 3].Value = report.MeterSerialNumber;
                        worksheet.Cells[row, 4].Value = report.ContentDetail.LastIndex;
                        worksheet.Cells[row, 5].Value = report.ContentDetail.ReadingTime;
                        worksheet.Cells[row, 5].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                        worksheet.Cells[row, 6].Value = report.ContentDetail.SerialNumber;
                        worksheet.Cells[row, 7].Value = report.ContentDetail.VoltageValue;
                        worksheet.Cells[row, 8].Value = report.ContentDetail.CurrentValue;
                        row++;
                    }
                }

                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                worksheet.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            }
        }
        public virtual byte[] GenerateCsv(List<ReportDownloadResponse[]> data)
        {
            var csv = new StringBuilder();

            csv.AppendLine("RequestDate,Status,MeterSerialNumber,LastIndex,ReadingTime,SerialNumber,VoltageValue,CurrentValue");

            foreach (var reportArray in data)
            {
                foreach (var report in reportArray)
                {
                    csv.AppendLine($"{report.RequestDate},{report.Status},{report.MeterSerialNumber},{report.ContentDetail.LastIndex},{report.ContentDetail.ReadingTime},{report.ContentDetail.SerialNumber},{report.ContentDetail.VoltageValue},{report.ContentDetail.CurrentValue}");
                }
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public virtual byte[] GenerateText(List<ReportDownloadResponse[]> data)
        {
            var text = new StringBuilder();

            text.AppendLine("RequestDate\tStatus\tMeterSerialNumber\tLastIndex\tReadingTime\tSerialNumber\tVoltageValue\tCurrentValue");

            foreach (var reportArray in data)
            {
                foreach (var report in reportArray)
                {
                    text.AppendLine($"{report.RequestDate}\t{report.Status}\t{report.MeterSerialNumber}\t{report.ContentDetail.LastIndex}\t{report.ContentDetail.ReadingTime}\t{report.ContentDetail.SerialNumber}\t{report.ContentDetail.VoltageValue}\t{report.ContentDetail.CurrentValue}");
                }
            }

            return Encoding.UTF8.GetBytes(text.ToString());
        }

    }
}