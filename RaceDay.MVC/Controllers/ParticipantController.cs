using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using System.Net;
using System.Net.Http.Json;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private IActionResult? CheckAccess()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Participant")
                return RedirectToAction("Index", "Home");

            return null;
        }

        private HttpRequestMessage CreateApiRequest(
            HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);

            var cookie = HttpContext.Session
                .GetString("ApiSessionCookie");

            if (!string.IsNullOrWhiteSpace(cookie))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie", cookie);
            }

            return request;
        }

        public IActionResult Index()
        {
            var access = CheckAccess();

            if (access != null)
                return access;

            ViewBag.UserID =
                HttpContext.Session.GetInt32("UserID");

            return View();
        }

        // Display available events and categories
        public async Task<IActionResult> Events()
        {
            var access = CheckAccess();

            if (access != null)
                return access;

            var events = new List<EventViewModel>();
            var categories = new List<CategoryViewModel>();

            try
            {
                var client = _httpClientFactory
                    .CreateClient("RaceDayApi");

                using var eventResponse =
                    await client.GetAsync("api/Events");

                if (eventResponse.IsSuccessStatusCode)
                {
                    events = await eventResponse.Content
                        .ReadFromJsonAsync<List<EventViewModel>>()
                        ?? new List<EventViewModel>();
                }
                else
                {
                    ViewBag.Error =
                        "Unable to load events.";
                }

                using var categoryResponse =
                    await client.GetAsync("api/Categories");

                if (categoryResponse.IsSuccessStatusCode)
                {
                    categories = await categoryResponse.Content
                        .ReadFromJsonAsync<List<CategoryViewModel>>()
                        ?? new List<CategoryViewModel>();
                }
                else
                {
                    ViewBag.Error =
                        "Unable to load event categories.";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.Error =
                    "The RaceDay service is unavailable.";
            }

            ViewBag.Categories = categories;

            return View(events);
        }

        // Display participant registrations
        public async Task<IActionResult> Registrations()
        {
            var access = CheckAccess();

            if (access != null)
                return access;

            var enrolments = new List<EnrolmentViewModel>();

            if (string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("ApiSessionCookie")))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _httpClientFactory
                    .CreateClient("RaceDayApi");

                using var request = CreateApiRequest(
                    HttpMethod.Get, "api/Enrolments");

                using var response =
                    await client.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Account");
                }

                if (response.IsSuccessStatusCode)
                {
                    enrolments = await response.Content
                        .ReadFromJsonAsync<List<EnrolmentViewModel>>()
                        ?? new List<EnrolmentViewModel>();
                }
                else
                {
                    ViewBag.Error =
                        "Unable to load your registrations.";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.Error =
                    "The RaceDay service is unavailable.";
            }

            return View(enrolments);
        }

        // Register for an event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterForEvent(
            int eventId, int categoryId)
        {
            var access = CheckAccess();

            if (access != null)
                return access;

            if (eventId <= 0 || categoryId <= 0)
            {
                TempData["Error"] =
                    "Please select a valid event and category.";

                return RedirectToAction(nameof(Events));
            }

            if (string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("ApiSessionCookie")))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _httpClientFactory
                    .CreateClient("RaceDayApi");

                using var request = CreateApiRequest(
                    HttpMethod.Post, "api/Enrolments");

                request.Content = JsonContent.Create(new
                {
                    EventID = eventId,
                    CategoryID = categoryId
                });

                using var response =
                    await client.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Account");
                }

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "You have successfully registered!";

                    return RedirectToAction(nameof(Registrations));
                }

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    TempData["Error"] =
                        "You are already registered for this event.";
                }
                else
                {
                    TempData["Error"] =
                        "Registration failed. Please check the category.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "The RaceDay service is unavailable.";
            }

            return RedirectToAction(nameof(Events));
        }

        // Display participant race results
        public async Task<IActionResult> Results()
        {
            var access = CheckAccess();

            if (access != null)
                return access;

            var results = new List<ResultViewModel>();

            if (string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("ApiSessionCookie")))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _httpClientFactory
                    .CreateClient("RaceDayApi");

                using var request = CreateApiRequest(
                    HttpMethod.Get, "api/Results");

                using var response =
                    await client.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Account");
                }

                if (response.IsSuccessStatusCode)
                {
                    results = await response.Content
                        .ReadFromJsonAsync<List<ResultViewModel>>()
                        ?? new List<ResultViewModel>();
                }
                else
                {
                    ViewBag.Error =
                        "Unable to load your results.";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.Error =
                    "The RaceDay service is unavailable.";
            }

            return View(results);
        }
    }
}
