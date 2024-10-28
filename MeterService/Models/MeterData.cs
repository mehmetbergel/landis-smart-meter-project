using System.ComponentModel.DataAnnotations;

namespace MeterService.Models
{
    public class MeterReading
    {
        [Key]
        public Guid UUID { get; set; }
        [MaxLength(8)]
        public string SerialNumber { get; set; }
        public DateTime ReadingTime { get; set; }
        public decimal LastIndex { get; set; }
        public decimal VoltageValue { get; set; }
        public decimal CurrentValue { get; set; }
    }
}

