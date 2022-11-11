using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nitro.Infrastructure.localization;

namespace Nitro.Web.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<HomeController> _logger;

        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IStringLocalizer<SharedResource> _sharedlocalizer;

        public HomeController(ILogger<HomeController> logger,
            IStringLocalizer<HomeController> localizer,
            IStringLocalizer<SharedResource> sharedlocalizer)
        {
            _logger = logger;
            _localizer = localizer;
            _sharedlocalizer = sharedlocalizer;
        }

        [HttpGet("GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation("First Log");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

        }
        [AllowAnonymous]
        [HttpGet("TestLocalization")]
        public string TestLocalization()
        {
            var str = _sharedlocalizer["Hello"];
            return str;
        }
    }
}