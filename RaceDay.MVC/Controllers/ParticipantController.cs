using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
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

        public IActionResult Index()
        {
            var accessResult = CheckAccess();

            if (accessResult != null)
                return accessResult;

            ViewBag.UserID =
                HttpContext.Session.GetInt32("UserID");

            return View();
        }

        public async Task<IActionResult> Events()
        {
            var accessResult = CheckAccess();

            if (accessResult != null)
                return accessResult;

            var events = new List<EventViewModel>();

            try
            {
                var client =
                    _httpClientFactory.CreateClient("RaceDayApi");

                using var response =
                    await client.GetAsync("api/Events");

                if (response.IsSuccessStatusCode)
                {
                    events = await response.Content
                        .ReadFromJsonAsync<List<EventViewModel>>()
                        ?? new List<EventViewModel>();
                }
                else
                {
                    ViewBag.Error =
                        "Unable to load events at the moment.";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.Error =
                    "The RaceDay service is unavailable.";
            }

            return View(events);
        }

        public async Task<IActionResult> Registrations()
        {
            var accessResult = CheckAccess();

            if (accessResult != null)
                return accessResult;

            var enrolments = new List<EnrolmentViewModel>();

            var apiSessionCookie =
                HttpContext.Session.GetString("ApiSessionCookie");

            if (string.IsNullOrWhiteSpace(apiSessionCookie))
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient("RaceDayApi");

                using var request = new HttpRequestMessage(
                    HttpMethod.Get, "api/Enrolments");

                // Authenticate this request with the API session.
                request.Headers.TryAddWithoutValidation(
                    "Cookie", apiSessionCookie);

                using var response =
                    await client.SendAsync(request);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
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
    }
}
