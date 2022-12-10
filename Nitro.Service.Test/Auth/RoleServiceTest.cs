using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Nitro.Core.Domain.Auth;
using Nitro.Core.Interfaces.Auth;
using Nitro.Core.Models.Auth;
using Nitro.Infrastructure.Data;
using Nitro.Infrastructure.Test;
using Nitro.Kernel.Interfaces.Data;
using Nitro.Service.Cms;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Xunit;

namespace Nitro.Service.Test.Auth
{
    public class RoleServiceTest : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly IFixture fixture;
        private readonly RoleService roleService;
        public RoleServiceTest()
        {
            //Arrange
            fixture = new Fixture().Customize(new AutoMoqCustomization());

            //fixture = new Fixture().Customize(new InMemoryCustomization());
            //var context = fixture.Create<ApplicationDbContext>();

            fixture.Freeze<Mock<ApplicationDbContext>>();

            var queryRepository = fixture.Freeze<Mock<IQueryRepository>>();
            var commandRepository = fixture.Freeze<Mock<ICommandRepository>>();

            


            var rolesList = CreateDbSetMock(fixture.CreateMany<Role>().AsQueryable());

            queryRepository.Setup(r => r.Table<Role>()).Returns(rolesList.Object);



            roleService = new RoleService(queryRepository.Object, commandRepository.Object);

        }

        [Fact]
        public async void retrieves_all_roles()
        {
            //Act
            var resultList = await roleService.GetList();

            //Assert
            resultList.Should().BeOfType<List<RoleModel>>();
        }

        [Theory]
        [InlineData(1)]
        public async void retrieve_role_by_id(int id)
        {
            //Act
            var resultList = await roleService.GetById(id);

            //Assert
            resultList.Should().BeOfType<RoleModel>();
        }

        private static Mock<DbSet<T>> CreateDbSetMock<T>(IQueryable<T> items) where T : class
        {
            var dbSetMock = new Mock<DbSet<T>>();

            dbSetMock.As<IAsyncEnumerable<T>>()
                .Setup(x => x.GetAsyncEnumerator(default))
                .Returns(new TestAsyncEnumerator<T>(items.GetEnumerator()));
            dbSetMock.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(items.Provider));
            dbSetMock.As<IQueryable<T>>()
                .Setup(m => m.Expression).Returns(items.Expression);
            dbSetMock.As<IQueryable<T>>()
                .Setup(m => m.ElementType).Returns(items.ElementType);
            dbSetMock.As<IQueryable<T>>()
                .Setup(m => m.GetEnumerator()).Returns(items.GetEnumerator());

            return dbSetMock;
        }
    }

}

