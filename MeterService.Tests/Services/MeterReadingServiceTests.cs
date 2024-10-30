using Xunit;
using MeterService.Services;
using MeterService.Models;
using System;

namespace MeterService.Tests.Services
{
    public class MeterReadingServiceTests
    {
        private readonly MeterReadingService _service;

        public MeterReadingServiceTests()
        {
            _service = new MeterReadingService();
        }

        [Fact]
        public void CreateMeterReading_ShouldCreateNewReadingWithGeneratedId()
        {
            var inputReading = new MeterReading
            {
                SerialNumber = "TEST1234",
                CurrentValue = 100.5m,
                LastIndex = 90.5m,
                VoltageValue = 220.0m,
                ReadingTime = DateTime.Now
            };

            var result = _service.CreateMeterReading(inputReading);

            Assert.NotEqual(Guid.Empty, result.UUID);
            Assert.Equal(inputReading.SerialNumber, result.SerialNumber);
            Assert.Equal(inputReading.CurrentValue, result.CurrentValue);
            Assert.Equal(inputReading.LastIndex, result.LastIndex);
            Assert.Equal(inputReading.VoltageValue, result.VoltageValue);
            Assert.True(result.ReadingTime <= DateTime.Now);
            Assert.True(result.ReadingTime > DateTime.Now.AddMinutes(-1));
        }

        [Fact]
        public void CreateMeterReading_ShouldHandleZeroValues()
        {
            var inputReading = new MeterReading
            {
                SerialNumber = "TEST1234",
                CurrentValue = 0m,
                LastIndex = 0m,
                VoltageValue = 0m,
                ReadingTime = DateTime.Now
            };

            var result = _service.CreateMeterReading(inputReading);

            Assert.NotEqual(Guid.Empty, result.UUID);
            Assert.Equal(inputReading.SerialNumber, result.SerialNumber);
            Assert.Equal(0m, result.CurrentValue);
            Assert.Equal(0m, result.LastIndex);
            Assert.Equal(0m, result.VoltageValue);
            Assert.True(result.ReadingTime <= DateTime.Now);
        }

        [Fact]
        public void CreateMeterReading_ShouldHandleMaxValues()
        {
            var inputReading = new MeterReading
            {
                SerialNumber = "TEST1234",
                CurrentValue = decimal.MaxValue,
                LastIndex = decimal.MaxValue,
                VoltageValue = decimal.MaxValue,
                ReadingTime = DateTime.Now
            };

            var result = _service.CreateMeterReading(inputReading);

            Assert.NotEqual(Guid.Empty, result.UUID);
            Assert.Equal(inputReading.SerialNumber, result.SerialNumber);
            Assert.Equal(decimal.MaxValue, result.CurrentValue);
            Assert.Equal(decimal.MaxValue, result.LastIndex);
            Assert.Equal(decimal.MaxValue, result.VoltageValue);
        }

        [Fact]
        public void CreateMeterReading_ShouldHandleMinValues()
        {
            var inputReading = new MeterReading
            {
                SerialNumber = "TEST1234",
                CurrentValue = decimal.MinValue,
                LastIndex = decimal.MinValue,
                VoltageValue = decimal.MinValue,
                ReadingTime = DateTime.Now
            };

            var result = _service.CreateMeterReading(inputReading);

            Assert.NotEqual(Guid.Empty, result.UUID);
            Assert.Equal(inputReading.SerialNumber, result.SerialNumber);
            Assert.Equal(decimal.MinValue, result.CurrentValue);
            Assert.Equal(decimal.MinValue, result.LastIndex);
            Assert.Equal(decimal.MinValue, result.VoltageValue);
        }
    }
}