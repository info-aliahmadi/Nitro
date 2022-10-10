using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Nitro.Test
{
    public class BasicTests
    {
        private readonly WebApplicationFactory<Program> _factory;
        public BasicTests()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // ... Configure test services
                });
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Index")]
        public async Task HelloWorldTest(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);


            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType?.ToString());


        }
    }
}