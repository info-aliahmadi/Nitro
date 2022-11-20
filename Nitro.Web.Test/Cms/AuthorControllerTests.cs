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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Nitro.Web.Test.Cms
{
    public class AuthorControllerTests : IClassFixture<ApiWebApplicationFactory>
    {
        readonly Fixture fixture;
        readonly AuthorController authorController;
        public AuthorControllerTests(ApiWebApplicationFactory application)
        {
            //Arrange
            fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization());
            authorController = fixture.Build<AuthorController>().OmitAutoProperties().Create();
        }

        [Fact]
        public async void Get_retrieves_authors()
        {
            // Fixture setup
            //var fixture = new Fixture();
            //var authorService = new Mock<IAuthorService>();
            //var authorController = new Mock<ILogger<AuthorController>>();

            //fixture.Register(() => authorService.Object);
            //fixture.Register(() => authorController.Object);

            //fixture.Customize(new AutoMoqCustomization());

            //var authorController = fixture.Build<AuthorController>().OmitAutoProperties().Create();

            //Arrange
            var expectedList = fixture.Create<ActionResult<IEnumerable<AuthorModel>>>();

            /*  Check By control expected values  */
            //var faceMock = fixture.Freeze<Mock<IFace>>();
            //faceMock.Setup(x => x.IsHappy()).Returns(true);


            //Act
            var resultList = await authorController.GetAuthors();

            //Assert
            resultList.Should().BeOfType<ActionResult<IEnumerable<AuthorModel>>>();
        }
        [Theory]
        [InlineData(0)]
        public async void Get_retrieve_author_by_zero(int id)
        {

            //Act
            var resultList = await authorController.GetAuthor(id);

            //Assert
            resultList.Should().BeOfType<NotFoundResult>();
        }
        [Theory]
        [InlineData(1)]
        public async void Get_retrieve_author_by_id(int id)
        {

            //Act
            var resultList = await authorController.GetAuthor(id);

            //Assert
            resultList.Should().BeOfType<OkObjectResult>();
        }
        [Theory, AutoData]
        public async void POST_add_author(AuthorModel authorModel)
        {

            //Act
            var resultList = await authorController.Add(authorModel);

            //Assert
            resultList.Should().BeOfType<OkObjectResult>();
        }
        [Theory, AutoData]
        public async void POST_update_author(AuthorModel authorModel)
        {

            //Act
            var resultList = await authorController.Update(authorModel);

            //Assert
            resultList.Should().BeOfType<ActionResult<AuthorModel>>();
        }
        [Theory, AutoData]
        public async void POST_delete_author(int authorId)
        {

            //Act
            var resultList = await authorController.Delete(authorId);

            //Assert
            resultList.Should().BeOfType<ActionResult<bool>>();
        }
    }
}