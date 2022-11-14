using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Nitro.Core.Interfaces.Cms;
using Nitro.Core.Models.Cms;
using Nitro.Infrastructure.Test;
using Nitro.Web.Controllers.Cms;
using System.Collections.Generic;
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
            var expectedList = fixture.CreateMany<AuthorModel>().ToList();

            //Act
            var resultList = sut.GetAuthors();

            //Assert
            resultList.Should().BeEquivalentTo(expectedList);
        }

        [Fact]
        public async void Test_Controller()
        {
            // Fixture setup
            var fixture = new Fixture();
            //var authorService = new Mock<IAuthorService>();
            //var authorController = new Mock<ILogger<AuthorController>>();

            //fixture.Register(() => authorService.Object);
            //fixture.Register(() => authorController.Object);

            fixture.Customize(new AutoMoqCustomization());

            var sut = fixture.Build<AuthorController>().OmitAutoProperties().Create();

            //Arrange
            var expectedList = fixture.Create<ActionResult<IEnumerable<AuthorModel>>>();

            /*  Check By control expected values  */
            //var faceMock = fixture.Freeze<Mock<IFace>>();
            //faceMock.Setup(x => x.IsHappy()).Returns(true);


            //Act
            var resultList = await sut.GetAuthors();

            //Assert
            resultList.Should().BeOfType();
        }
    }
}