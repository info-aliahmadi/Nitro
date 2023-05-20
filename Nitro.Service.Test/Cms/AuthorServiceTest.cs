using AutoFixture.AutoMoq;
using AutoFixture;
using Moq;
using Nitro.Infrastructure.Data;
using Nitro.Infrastructure.Test;
using Nitro.Kernel.Interfaces.Data;
using Nitro.Service.Cms;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Nitro.Core.Data.Domain;
using Nitro.Core.Models.Cms;
using FluentAssertions;

namespace Nitro.Service.Test.Cms
{
    public class AuthorServiceTest : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly IFixture fixture;
        private readonly AuthorService authorService;
        private IEnumerable<Author> authorItems;

        public AuthorServiceTest()
        {
            //Arrange
            fixture = new Fixture().Customize(new AutoMoqCustomization());
            fixture.Freeze<Mock<ApplicationDbContext>>();

            var queryRepository = fixture.Freeze<Mock<IQueryRepository>>();
            var commandRepository = fixture.Freeze<Mock<ICommandRepository>>();

            authorItems = fixture.CreateMany<Author>();

            var authorDbSet = DbSetMockExtension.CreateDbSetMock<Author>(authorItems.AsQueryable());

            queryRepository.Setup(x => x.Table<Author>()).Returns(authorDbSet.Object);

            authorService = new AuthorService(queryRepository.Object, commandRepository.Object);
        }

        [Fact]
        public async void GetList_RetrieveAuthors_ReturnsAuthorModelList()
        {
            //Act
            var resultList = await authorService.GetList();

            //Assert
            resultList.Should().BeOfType<List<AuthorModel>>();
        }

        [Theory]
        [InlineData(1)]
        public async void GetById_RetrieveAuthorById_ReturnsAuthorModel(int id)
        {
            //Act
            var resultList = await authorService.GetById(id);

            //Assert
            resultList.Should().BeOfType<AuthorModel>();
        }

        [Fact]
        public async void Add_InsertAuthor_ReturnsAuthorModel()
        {
            //Arrange
            var authorModel = fixture.Create<AuthorModel>();
            //Act
            var resultAuthorModel = await authorService.Add(authorModel);

            //Assert
            resultAuthorModel.Should().BeEquivalentTo(authorModel);
        }

        [Fact]
        public async void Update_UpdateAuthorFields_ReturnsAuthorModel()
        {
            //Arrange
            var authorModel = fixture.Create<AuthorModel>();
            authorModel.Id = authorItems.First().Id;

            //Act
            var resultAuthorModel = await authorService.Update(authorModel);

            //Assert
            resultAuthorModel.Should().BeEquivalentTo(authorModel);
        }

        [Fact]
        public async void Delete_DeleteAuthorFields_ReturnsAuthorModel()
        {
            //
            //
            var authorModel = fixture.Create<AuthorModel>();
            authorModel.Id = authorItems.First().Id;

            //Act
            var resultAuthorModel = await authorService.Update(authorModel);

            //Assert
            resultAuthorModel.Should().BeEquivalentTo(authorModel);
        }


    }
}
