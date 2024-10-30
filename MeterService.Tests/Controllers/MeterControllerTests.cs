using Xunit;
using Moq;
using MeterService.Controllers;
using MeterService.Services;
using MeterService.Models;
using MeterService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeterService.Tests.Controllers
{
    public class MeterControllerTests
    {
        private readonly Mock<MeterReadingService> _mockService;
        private readonly ApplicationDbContext _context;
        private readonly MeterController _controller;

        public MeterControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _mockService = new Mock<MeterReadingService>();
            _controller = new MeterController(_context, _mockService.Object);
        }

        [Fact]
        public async Task GetMeterReadings_ReturnsAllReadings()
        {
            var readings = new List<MeterReading>
            {
                new MeterReading { UUID = Guid.NewGuid(), SerialNumber = "TEST1234" },
                new MeterReading { UUID = Guid.NewGuid(), SerialNumber = "TEST2345" }
            };
            await _context.MeterReadings.AddRangeAsync(readings);
            await _context.SaveChangesAsync();

            var result = await _controller.GetMeterReadings();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<MeterReading>>>(result);
            var returnedReadings = Assert.IsAssignableFrom<IEnumerable<MeterReading>>(actionResult.Value);
            Assert.Equal(2, returnedReadings.Count());
        }

        [Fact]
        public void GetMeterReading_ReturnsReadingById()
        {
            var reading = new MeterReading { UUID = Guid.NewGuid(), SerialNumber = "TEST1234" };
            _context.MeterReadings.Add(reading);
            _context.SaveChanges();

            var result = _controller.GetMeterReading("TEST1234");

            var actionResult = Assert.IsType<ActionResult<MeterReading>>(result);
            var returnedReading = Assert.IsType<MeterReading>(actionResult.Value);
            Assert.Equal("TEST1234", returnedReading.SerialNumber);
        }

        [Fact]
        public async Task PostMeterReading_CreatesNewReading()
        {
            var newReading = new MeterReading
            {
                SerialNumber = "TEST1234",
                CurrentValue = 100.5m
            };

            var createdReading = new MeterReading
            {
                UUID = Guid.NewGuid(),
                SerialNumber = "TEST1234",
                CurrentValue = 100.5m
            };

            _mockService
                .Setup(s => s.CreateMeterReading(It.IsAny<MeterReading>()))
                .Returns(createdReading);

            var result = await _controller.PostMeterReading(newReading);

            var actionResult = Assert.IsType<ActionResult<MeterReading>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnedReading = Assert.IsType<MeterReading>(createdAtActionResult.Value);
            Assert.Equal(createdReading.SerialNumber, returnedReading.SerialNumber);
        }

        [Fact]
        public async Task DeleteMeterReading_RemovesReading()
        {
            var reading = new MeterReading { UUID = Guid.NewGuid(), SerialNumber = "TEST1234" };
            _context.MeterReadings.Add(reading);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteMeterReading(reading.UUID);

            Assert.IsType<NoContentResult>(result);
            Assert.Null(await _context.MeterReadings.FindAsync(reading.UUID));
        }

        [Fact]
        public void GetMeterReading_ReturnsNotFound_WhenReadingDoesNotExist()
        {
            var result = _controller.GetMeterReading("NONEXISTENT");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PutMeterReading_ReturnsNoContent_WhenUpdateSuccessful()
        {
            var reading = new MeterReading 
            { 
                UUID = Guid.NewGuid(), 
                SerialNumber = "TEST1234",
                CurrentValue = 100.5m
            };
            _context.MeterReadings.Add(reading);
            await _context.SaveChangesAsync();

            reading.CurrentValue = 200.5m;

            var result = await _controller.PutMeterReading(reading.UUID, reading);

            Assert.IsType<NoContentResult>(result);
            var updatedReading = await _context.MeterReadings.FindAsync(reading.UUID);
            Assert.Equal(200.5m, updatedReading.CurrentValue);
        }

        [Fact]
        public async Task PutMeterReading_ReturnsBadRequest_WhenIdsDoNotMatch()
        {
            var reading = new MeterReading 
            { 
                UUID = Guid.NewGuid(), 
                SerialNumber = "TEST1234" 
            };

            var result = await _controller.PutMeterReading(Guid.NewGuid(), reading);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PutMeterReading_ReturnsNotFound_WhenReadingDoesNotExist()
        {
            var reading = new MeterReading 
            { 
                UUID = Guid.NewGuid(), 
                SerialNumber = "TEST1234" 
            };

            var result = await _controller.PutMeterReading(reading.UUID, reading);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteMeterReading_ReturnsNotFound_WhenReadingDoesNotExist()
        {
            var result = await _controller.DeleteMeterReading(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }
    }
}