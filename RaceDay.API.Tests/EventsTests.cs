using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests
{
    public class EventsTests
    {
        // TEST 6: Participant cannot create an event
        [Fact]
        public async Task CreateEvent_Participant_ReturnsForbidden()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new RaceDayDbContext(options);
            var controller = new EventsController(context);

            var session = new TestSession();
            session.SetInt32("UserID", 5);
            session.SetString("Role", "Participant");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    Session = session
                }
            };

            var raceEvent = new Event
            {
                EventName = "Test Running Event",
                Description = "A running event for testing",
                EventDate = new DateTime(2026, 12, 10),
                Location = "Johannesburg",
                Distance = 10,
                EventTypeID = 1
            };

            // Act
            var result = await controller.CreateEvent(raceEvent);

            // Assert
            var response = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(403, response.StatusCode);
            Assert.Empty(await context.Events.ToListAsync());
        }

        // TEST 7: Organiser can create an event
        [Fact]
        public async Task CreateEvent_Organiser_ReturnsCreated()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new RaceDayDbContext(options);
            var controller = new EventsController(context);

            var session = new TestSession();
            session.SetInt32("UserID", 6);
            session.SetString("Role", "Organiser");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    Session = session
                }
            };

            var raceEvent = new Event
            {
                EventName = "Pretoria Fun Run",
                Description = "A running event for participants",
                EventDate = new DateTime(2026, 12, 15),
                Location = "Pretoria",
                Distance = 5,
                EventTypeID = 1
            };

            // Act
            var result = await controller.CreateEvent(raceEvent);

            // Assert
            var response = Assert.IsType<CreatedAtActionResult>(
                result.Result);

            Assert.Equal(201, response.StatusCode);

            var savedEvent = await context.Events.SingleAsync();

            Assert.Equal("Pretoria Fun Run", savedEvent.EventName);
            Assert.Equal(6, savedEvent.OrganiserID);
        }
    }
}
