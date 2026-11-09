using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using System.Net;
using System.Net.Http.Json;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrganiserController(
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

            if (role != "Organiser")
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

        private IActionResult SessionExpired()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Index()
        {
            var accessResult = CheckAccess();

            if (accessResult != null)
                return accessResult;

            return View();
        }

        // Display events belonging to the organiser
        [HttpGet]
        public async Task<IActionResult> Events()
        {
            var accessResult = CheckAccess();

            if (accessResult != null)
                return accessResult;

            var events = new List<ManageEventViewModel>();

            try
            {
                var client = _httpClientFactory
                    .CreateClient("RaceDayApi");

                using var response =
                    await client.GetAsync("api/Events");

                if (response.IsSuccessStatusCode)
                {
                    var allEvents = await response.Content
                        .ReadFromJsonAsync<
                            List<ManageEventViewModel>>()
                        ?? new List<ManageEventViewModel>();

                    var organiserId = HttpContext.Session
                        .GetInt32("UserID");

                    events = allEvents
                        .Where(e => e.OrganiserID == organiserId)
                        .ToList();
                }
                else
                {
                    ViewBag.Error = "Unable to load events.";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.Error =
                    "The RaceDay service is unavailable.";
            }

            return View(events);
        }

        // Display create event form
        [HttpGet]
        public IActionResult CreateEvent()
        {
            var accessResult = CheckAccess();

            if (accessResult != null)
                return accessResult;

            return View(new ManageEventViewModel());
        }

        // Save new event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(
            ManageEventViewModel model)
        {
            var accessResult = CheckAccess();

            if (accessResult != null)
                return accessResult;

            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("ApiSessionCookie")))
                return SessionExpired();

            try
            {
                var client = _httpClientFactory
                    .CreateClient("RaceDayApi");

                using var request = CreateApiRequest(
                    HttpMethod.Post, "api/Events");

                request.Content = JsonContent.Create(new
                {
                    model.EventTypeID,
                    model.EventName,
                    model.Description,
                    model.EventDate,
                    model.Location,
                    model.Distance
                });

                using var response =
                    await client.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return SessionExpired();

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Event created successfully.";

                    return RedirectToAction(nameof(Events));
                }

                ModelState.AddModelError(
                    "",
                    "Unable to create event. Check the details and event type.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "The RaceDay service is unavailable.");
            }

            return View(model);
        }

        // Display edit event form
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var accessResult = CheckAccess();

            if (accessResult != null)
                return accessResult;

            try
            {
                var client = _httpClientFactory
                    .CreateClient("RaceDayApi");

                using var response =
                    await client.GetAsync($"api/Events/{id}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return NotFound();

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] =
                        "Unable to load event.";

                    return RedirectToAction(nameof(Events));
                }

                var model = await response.Content
                    .ReadFromJsonAsync<ManageEventViewModel>();

                if (model == null)
                    return NotFound();

                var organiserId = HttpContext.Session
                    .GetInt32("UserID");

                if (model.OrganiserID != organiserId)
                    return Forbid();

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "The RaceDay service is unavailable.";

                return RedirectToAction(nameof(Events));
            }
        }

        // Save event changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(
            int id, ManageEventViewModel model)
        {
            var accessResult = CheckAccess();

            if (accessResult != null)
                return accessResult;

            if (id != model.EventID)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("ApiSessionCookie")))
                return SessionExpired();

            try
            {
                var client = _httpClientFactory
                    .CreateClient("RaceDayApi");

                using var request = CreateApiRequest(
                    HttpMethod.Put, $"api/Events/{id}");

                request.Content = JsonContent.Create(model);

                using var response =
                    await client.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return SessionExpired();

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    return Forbid();

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return NotFound();

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Event updated successfully.";

                    return RedirectToAction(nameof(Events));
                }

                ModelState.AddModelError(
                    "",
                    "Unable to update event. Please check the details.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "The RaceDay service is unavailable.");
            }

            return View(model);
        }

        // Delete an event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var accessResult = CheckAccess();

            if (accessResult != null)
                return accessResult;

            if (string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("ApiSessionCookie")))
                return SessionExpired();

            try
            {
                var client = _httpClientFactory
                    .CreateClient("RaceDayApi");

                using var request = CreateApiRequest(
                    HttpMethod.Delete, $"api/Events/{id}");

                using var response =
                    await client.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return SessionExpired();

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Event deleted successfully.";
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["Error"] =
                        "You can only delete your own events.";
                }
                else
                {
                    TempData["Error"] =
                        "Unable to delete event. It may have registrations.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "The RaceDay service is unavailable.";
            }

            return RedirectToAction(nameof(Events));
        }
    }
}
