using ASP.NET_IoT.Models.Mqtt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ASP.NET_IoT.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IMemoryCache cache, ILogger<DashboardController> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public IActionResult Index(string area, string zone)
        {
            if (string.IsNullOrEmpty(area) || string.IsNullOrEmpty(zone))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["Area"] = area;
            ViewData["Zone"] = zone;

            var cacheKey = $"latest_{area}_{zone}";
            if (_cache.TryGetValue(cacheKey, out SensorPayload? payload))
            {
                return View(payload);
            }

            _logger.LogInformation("Sensor data not found in cache for {Area}/{Zone}", area, zone);
            return View(null);
        }
    }
}
