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

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Participant")
                return RedirectToAction("Index", "Home");

            ViewBag.UserID = userId;

            return View();
        }

        public async Task<IActionResult> Events()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role != "Participant")
                return RedirectToAction("Index", "Home");

            var events = new List<EventViewModel>();

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayApi");

                var response = await client.GetAsync("api/Events");

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
            catch (TaskCanceledException)
            {
                ViewBag.Error =
                    "The RaceDay service took too long to respond.";
            }
            catch (System.Text.Json.JsonException)
            {
                ViewBag.Error =
                    "Unable to read event information.";
            }

            return View(events);
        }
    }
}
