using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string firstName, string lastName, string email,
            string password, string role, string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                (role != "Organiser" && role != "Participant"))
            {
                ViewBag.Error = "Please enter valid registration details.";
                return View();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayApi");

                var response = await client.PostAsJsonAsync(
                    "api/Auth/register",
                    new
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        Password = password,
                        Role = role,
                        PhoneNumber = phoneNumber
                    });

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = response.StatusCode == HttpStatusCode.Conflict
                        ? "This email address is already registered."
                        : "Registration failed. Please check your details.";

                    return View();
                }

                TempData["Success"] =
                    "Registration successful. Please log in.";

                return RedirectToAction(nameof(Login));
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "The RaceDay service is unavailable.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayApi");

                using var response = await client.PostAsJsonAsync(
                    "api/Auth/login",
                    new
                    {
                        Email = email,
                        Password = password
                    });

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Invalid email or password.";
                    return View();
                }

                var user = await response.Content
                    .ReadFromJsonAsync<LoginResponse>();

                if (user == null ||
                    (user.Role != "Organiser" &&
                     user.Role != "Participant"))
                {
                    ViewBag.Error = "Unable to complete login.";
                    return View();
                }

                // Retrieve the session cookie created by the API.
                string? apiSessionCookie = null;

                if (response.Headers.TryGetValues(
                    "Set-Cookie", out var cookies))
                {
                    apiSessionCookie = cookies
                        .Select(cookie => cookie.Split(';')[0].Trim())
                        .FirstOrDefault(cookie =>
                            cookie.StartsWith(
                                ".AspNetCore.Session=",
                                StringComparison.Ordinal));
                }

                if (string.IsNullOrWhiteSpace(apiSessionCookie))
                {
                    ViewBag.Error =
                        "Unable to establish an API session.";
                    return View();
                }

                // Save MVC login details and the API session.
                HttpContext.Session.Clear();

                HttpContext.Session.SetInt32(
                    "UserID", user.UserID);

                HttpContext.Session.SetString(
                    "Role", user.Role);

                HttpContext.Session.SetString(
                    "ApiSessionCookie", apiSessionCookie);

                return user.Role == "Organiser"
                    ? RedirectToAction("Index", "Organiser")
                    : RedirectToAction("Index", "Participant");
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "The RaceDay service is unavailable.";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }

        private class LoginResponse
        {
            public int UserID { get; set; }

            public string Role { get; set; } = "";
        }
    }
}
