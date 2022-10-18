using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using Nitro.Core.Interfaces.Cms;
using Nitro.Core.Models.Cms;
using Nitro.Web.Controllers.Cms;
using Xunit;

namespace Nitro.Web.Test.Controllers.Cms
{
    public class AuthorControllerTest
    {
        public  IList<AuthorModel> _authors2 = new List<AuthorModel>()
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
            // Fixture setup
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization());

            var authors = fixture.Freeze<List<AuthorModel>>();
            var authorService = fixture.Freeze<Mock<IAuthorService>>();

            var sut = fixture.Create<IAuthorService>();

            sut.GetList().Verify(x => x.GetList());

            var sss =  productRepo.Setup(x => x.GetList().Result).Returns(() => _authors);

            sss.Should().BeEquivalentTo(_authors);
        }
    }
}
