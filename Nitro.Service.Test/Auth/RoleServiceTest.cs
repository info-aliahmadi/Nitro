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

            fixture.Freeze<Mock<ApplicationDbContext>>();

            var queryRepository = fixture.Freeze<Mock<IQueryRepository>>();
            var commandRepository = fixture.Freeze<Mock<ICommandRepository>>();

            var items = fixture.CreateMany<Role>();


                         queryRepository.As<IAsyncEnumerable<Role>>()
                .Setup(x => x.GetAsyncEnumerator(default))
                .Returns(new AsyncEnumerator<Role>(items.GetEnumerator()));

            queryRepository.As<IQueryable<Role>>()
                .Setup(m => m.Provider)
                .Returns(new AsyncQueryProvider<T>(items.Provider));

            queryRepository.As<IQueryable<T>>()
                .Setup(m => m.Expression).Returns(items.Expression);

            queryRepository.As<IQueryable<T>>()
                .Setup(m => m.ElementType).Returns(items.ElementType);

            queryRepository.As<IQueryable<T>>()
                .Setup(m => m.GetEnumerator()).Returns(items.GetEnumerator());

            queryRepository.Setup(x => x.Table<Role>())
         .Returns(fixture.CreateMany<Role>().AsQueryable());

            //var rolesList = DbSetMockExtension.CreateDbSetMock(fixture.CreateMany<Role>().AsQueryable());

            //queryRepository.Setup(r => r.Table<Role>()).Returns(rolesList.Object);

            roleService = new RoleService(queryRepository.Object, commandRepository.Object);

        }

        [Fact]
        public async void GetList_RetrieveRoles_ReturnsRoleModelList()
        {
            //Act
            var resultList = await roleService.GetList();

            //Assert
            resultList.Should().BeOfType<List<RoleModel>>();
        }

        [Theory]
        [InlineData(1)]
        public async void GetById_RetrieveRoleById_ReturnsRoleModel(int id)
        {
            //Act
            var resultList = await roleService.GetById(id);

            //Assert
            resultList.Should().BeOfType<RoleModel>();
        }

        [Fact]
        public async void Add_InsertRole_ReturnsRoleModel()
        {
            //Arrange
            var roleModel = fixture.Create<RoleModel>();
            //Act
            var resultRoleModel = await roleService.Add(roleModel);

            //Assert
            resultRoleModel.Should().BeEquivalentTo(roleModel);
        }

        [Fact]
        public async void Update_UpdateRoleFields_ReturnsRoleModel()
        {
            //Arrange
            var roleModel = fixture.Create<RoleModel>();
            //Act
            var resultRoleModel = await roleService.Update(roleModel);

            //Assert
            resultRoleModel.Should().BeEquivalentTo(roleModel);
        }


    }

}

