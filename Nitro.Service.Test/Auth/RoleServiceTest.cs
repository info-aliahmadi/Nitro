using System;
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
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Nitro.Infrastructure.Data;
using Nitro.Kernel.Extensions;
using Xunit;

namespace Nitro.Service.Test.Auth
{
    public class RoleServiceTest : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Mock<IQueryRepository> queryRepository = new Mock<IQueryRepository>();
        private readonly Mock<ICommandRepository> commandRepository = new Mock<ICommandRepository>();

        private RoleService RoleService => new RoleService(queryRepository.Object, commandRepository.Object);

        readonly Fixture fixture;
        readonly RoleService roleService;
        public RoleServiceTest()
        {
            //Arrange
            fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization());

           var ssssd = fixture.CreateMany<Role>();

            var qq = new Mock<DbSet<Role>>();

            //queryRepository.Setup(mr => mr.Table<Role>()).Returns(ssssd.AsQueryable());


            //fixture.Freeze<Mock<IQueryRepository>>();
            //fixture.Freeze<Mock<ICommandRepository>>();
            //roleService =new RoleService(mockQueryRepository.Object, mockCommandRepository.Object);//.OmitAutoProperties().Create();
        }

        [Fact]
        public async void GET_retrieves_all_roles()
        {
            //Act
            var resultList = await RoleService.GetList3();

            //Assert
            resultList.Should().BeOfType<List<RoleModel>>();
        }

    }
}