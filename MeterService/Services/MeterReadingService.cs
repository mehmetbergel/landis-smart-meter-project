using MeterService.Models;

namespace MeterService.Services
{
    public class MeterReadingService
    {
        public MeterReading CreateMeterReading(MeterReading meterReading)
        {
            return new MeterReading
            {
                UUID = Guid.NewGuid(),
                SerialNumber = meterReading.SerialNumber,
                CurrentValue = meterReading.CurrentValue,
                LastIndex = meterReading.LastIndex,
                VoltageValue = meterReading.VoltageValue,
                ReadingTime = DateTime.Now
            };
        }
    }
}
