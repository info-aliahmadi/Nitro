using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Nitro.Infrastructure.Test;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Nitro.Web.Test
{
    public class ApiTests : IClassFixture<ApiWebApplicationFactory>
    {
        readonly HttpClient _client;
        public ApiTests(ApiWebApplicationFactory application)
        {
            _client = application.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(scheme: "TestScheme");
        }
        [Fact]
        public async Task GET_retrieves_authors()
        {
            var response = await _client.GetAsync("/Api/Cms/Author/GetAuthors");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        [Fact]
        public async Task GET_retrieves_author_by_id_zero()
        {
            var response = await _client.GetAsync("/Api/Cms/Author/GetAuthor?authorId=0");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}