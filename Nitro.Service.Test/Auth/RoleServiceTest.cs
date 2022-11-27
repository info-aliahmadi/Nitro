using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Nitro.Core.Domain.Auth;
using Nitro.Core.Models.Auth;
using Nitro.Infrastructure.Test;
using Nitro.Kernel.Interfaces.Data;
using Nitro.Service.Cms;
using System.Collections.Generic;
using Xunit;

namespace Nitro.Service.Test.Auth
{
    public class RoleServiceTest : IClassFixture<ApiWebApplicationFactory>
    {
        readonly Fixture fixture;
        readonly RoleService roleService;
        public RoleServiceTest()
        {
            //Arrange
            fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization());
            var repostory = fixture.Freeze<Mock<IQueryRepository>>();
            repostory.Setup(r => r.Table<Role>()).Returns(new DbSet<Role>());
            //fixture.Freeze<Mock<IQueryRepository>>();
            //fixture.Freeze<Mock<ICommandRepository>>();
            roleService = fixture.Create<RoleService>();//.OmitAutoProperties().Create();
        }

        [Fact]
        public async void GET_retrieves_all_roles()
        {
            //Act
            var resultList = await roleService.GetList();

            //Assert
            resultList.Should().BeOfType<List<RoleModel>>();
        }

    }
}