using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Nitro.Core.Interfaces.Cms;
using Nitro.Core.Models.Cms;
using Nitro.Web.Controllers.Cms;
using Xunit;

namespace Nitro.Web.Test.Controllers.Cms
{
    public class AuthorControllerTest
    {
        public  IList<AuthorModel> _authors = new List<AuthorModel>()
        {
            new ()
            {
                Id = 1,
                FirstName = "fname1",
                LastName = "lname1",
                UserName = "username1",
                UserId = 1
            },
            new ()
            {
                Id = 2,
                FirstName = "fname2",
                LastName = "lname2",
                UserName = "username2",
                UserId = 2
            },
            new ()
            {
                Id = 2,
                FirstName = "fname2",
                LastName = "lname2",
                UserName = "username2",
                UserId = 2
            }
        };
        [Fact]
        public async Task GetTest()
        {
            // arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var service = fixture.Freeze<Mock<IAuthorService>>();
            service.Setup(a => a.GetList())
                .ReturnsAsync(_authors);
            var controller = fixture.Build<AuthorController>().OmitAutoProperties().Create();

            // act
            var response = await controller.GetNumberOfCharacters("Hoi");

            // assert
            Assert.Equal(8, ((OkObjectResult)response.Result).Value);
            service.Verify(s => s.GetNumberOfCharactersFromSearchQuery("Hoi"), Times.Once);
        }
    }
}
