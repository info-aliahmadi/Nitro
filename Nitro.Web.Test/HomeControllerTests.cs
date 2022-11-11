using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Nitro.Infrastructure.Test;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Nitro.Web.Test
{
    public class HomeControllerTests : IClassFixture<ApiWebApplicationFactory>
    {
        readonly HttpClient _client;
        public HomeControllerTests(ApiWebApplicationFactory application)
        {
            _client = application.CreateClient();
        }
        [Fact]
        public async Task GET_retrieves_weather_forecast()
        {
            var response = await _client.GetAsync("/api/Home/GetWeatherForecast");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        [Fact]
        public async Task GET_retrieves_TestLocalization()
        {
            var response = await _client.GetAsync("/api/Home/TestLocalization");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}