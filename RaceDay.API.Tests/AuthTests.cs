using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests
{
    public class AuthTests
    {
        // TEST 1: Check that invalid roles are rejected
        [Fact]
        public async Task Register_InvalidRole_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new RaceDayDbContext(options);
            var controller = new AuthController(context);

            var request = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Student",
                Email = "student@example.com",
                Password = "TestPass2026!",
                PhoneNumber = "0723456789",
                Role = "Admin"
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // TEST 2: Check that a Participant can register
        [Fact]
        public async Task Register_ValidParticipant_ReturnsCreated()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new RaceDayDbContext(options);
            var controller = new AuthController(context);

            var request = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@example.com",
                Password = "TestPass2026!",
                PhoneNumber = "0723456789",
                Role = "Participant"
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, createdResult.StatusCode);

            var savedUser = await context.Users.SingleAsync();

            Assert.Equal("Participant", savedUser.Role);
            Assert.NotEqual(request.Password, savedUser.PasswordHash);
        }

        // TEST 3: Check that duplicate emails are rejected
        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new RaceDayDbContext(options);
            var controller = new AuthController(context);

            var request = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "duplicate@example.com",
                Password = "TestPass2026!",
                PhoneNumber = "0723456789",
                Role = "Participant"
            };

            // Register the first user
            await controller.Register(request);

            // Act: Try registering with the same email again
            var result = await controller.Register(request);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(1, await context.Users.CountAsync());
        }

        // TEST 4: Check that an incorrect password is rejected
        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new RaceDayDbContext(options);
            var controller = new AuthController(context);

            var registration = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "login@example.com",
                Password = "CorrectPass2026!",
                PhoneNumber = "0723456789",
                Role = "Participant"
            };

            // Register a user with the correct password
            await controller.Register(registration);

            var login = new LoginRequest
            {
                Email = "login@example.com",
                Password = "WrongPass2026!"
            };

            // Act: Attempt login with the wrong password
            var result = await controller.Login(login);

            // Assert: Login must be rejected
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // TEST 5: Check that a Participant can log in successfully
        [Fact]
        public async Task Login_ValidParticipant_ReturnsOk()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new RaceDayDbContext(options);
            var controller = new AuthController(context);

            var user = new User
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "success@example.com",
                PhoneNumber = "0723456789",
                Role = "Participant"
            };

            var passwordHasher = new PasswordHasher<User>();

            user.PasswordHash = passwordHasher.HashPassword(
                user, "TestPass2026!");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Create a temporary session
            var session = new TestSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            controller.HttpContext.Session = session;

            var login = new LoginRequest
            {
                Email = "success@example.com",
                Password = "TestPass2026!"
            };

            // Act
            var result = await controller.Login(login);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(user.UserID, session.GetInt32("UserID"));
            Assert.Equal("Participant", session.GetString("Role"));
        }
    }
}
