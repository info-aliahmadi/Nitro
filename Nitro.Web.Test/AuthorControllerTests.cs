using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Nitro.Core.Interfaces.Cms;
using Nitro.Core.Models.Cms;
using Nitro.Infrastructure.Test;
using Nitro.Web.Controllers.Cms;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Nitro.Web.Test
{
    public class AuthorControllerTests : IClassFixture<ApiWebApplicationFactory>
    {
        readonly HttpClient _client;
        public AuthorControllerTests(ApiWebApplicationFactory application)
        {
            _client = application.CreateClient();
        }
        [Fact]
        public async Task GET_retrieves_GetAuthors()
        {
            var response = await _client.GetAsync("/Api/Cms/GetAuthors");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory, AutoData]
        public void GetWhenHasCustomerTypesShouldReturnOneCustomerType(
        IFixture fixture,
        [Frozen] Mock<IAuthorService> service,
        [Frozen] Mock<ILogger<AuthorController>> logger,
        [Greedy] AuthorController sut)
        {
            //Arrange
            var items = fixture.CreateMany<AuthorModel>(3).ToList();

            //Act
            var result = sut.GetAuthors();

            //Assert
            result.Should().BeEquivalentTo(items);
        }
    }
}