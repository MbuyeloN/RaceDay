using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests
{
    public class EnrolmentsTests
    {
        // TEST 8: Organisers cannot enrol in events
        [Fact]
        public async Task CreateEnrolment_Organiser_ReturnsForbidden()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new RaceDayDbContext(options);
            var controller = new EnrolmentsController(context);

            var session = new TestSession();
            session.SetInt32("UserID", 6);
            session.SetString("Role", "Organiser");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = session
                }
            };

            var enrolment = new Enrolment
            {
                ParticipantID = 6,
                EventID = 1,
                CategoryID = 1
            };

            // Act
            var result = await controller.CreateEnrolment(enrolment);

            // Assert
            var response = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, response.StatusCode);
            Assert.Empty(await context.Enrolments.ToListAsync());
        }

        // TEST 9: Participants can enrol successfully
        [Fact]
        public async Task CreateEnrolment_Participant_ReturnsCreated()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new RaceDayDbContext(options);

            // Create a test event
            var raceEvent = new Event
            {
                OrganiserID = 1,
                EventTypeID = 1,
                EventName = "Test Race",
                Description = "Running event",
                EventDate = new DateTime(2026, 12, 15),
                Location = "Pretoria",
                Distance = 10
            };

            context.Events.Add(raceEvent);
            await context.SaveChangesAsync();

            // Create a category linked to the event
            var category = new Category
            {
                EventID = raceEvent.EventID,
                CategoryName = "10km"
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var controller = new EnrolmentsController(context);

            var session = new TestSession();
            session.SetInt32("UserID", 5);
            session.SetString("Role", "Participant");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = session
                }
            };

            var enrolment = new Enrolment
            {
                EventID = raceEvent.EventID,
                CategoryID = category.CategoryID
            };

            // Act
            var result = await controller.CreateEnrolment(enrolment);

            // Assert
            var response = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, response.StatusCode);

            var savedEnrolment =
                await context.Enrolments.SingleAsync();

            Assert.Equal(5, savedEnrolment.ParticipantID);
            Assert.Equal(raceEvent.EventID, savedEnrolment.EventID);
            Assert.Equal(category.CategoryID, savedEnrolment.CategoryID);
        }
    }
}
