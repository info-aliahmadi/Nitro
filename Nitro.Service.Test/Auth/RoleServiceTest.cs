using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Nitro.Core.Domain.Auth;
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
        private static Mock<DbSet<T>> CreateDbSetMock<T>(IEnumerable<T> elements) where T : class
        {
            var elementsAsQueryable = elements.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();

            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(elementsAsQueryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(elementsAsQueryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(elementsAsQueryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(elementsAsQueryable.GetEnumerator());

            return dbSetMock;
        }

        //readonly Fixture fixture;
        //readonly RoleService roleService;
        public RoleServiceTest()
        {
            //Arrange
            //fixture = new Fixture();
            //fixture.Customize(new AutoMoqCustomization());
            ////var dbRole = new Mock<DbSet<Role>>();
            ////var applicationDbContext = new Mock<ApplicationDbContext>();

            ////fixture.Freeze<Mock<ApplicationDbContext>>();
            //fixture.Freeze<Mock<QueryRepository>>();
            ////fixture.Freeze<Mock<CommandRepository>>();
            ////fixture.Freeze<Mock<CommandRepository>>();
            //////repostory.Setup(r => r.Table<Role>()).Returns(dbRole.Object);
            //fixture.Freeze<Mock<IQueryRepository>>();
            //fixture.Freeze<Mock<ICommandRepository>>();
            ////roleService = fixture.Create<RoleService>();//.OmitAutoProperties().Create();

            //var queryRepository = new Mock<IQueryRepository>();
            //var commandRepository = new Mock<ICommandRepository>();
            //roleService = new RoleService(queryRepository.Object, commandRepository.Object);



            //var rolesMock = CreateDbSetMock(fixture.CreateMany<Role>());
            //var userContextMock = new Mock<ApplicationDbContext>();
            //userContextMock.Setup(x => x.Role).Returns(rolesMock.Object);

            //roleService = new RoleService(queryRepository.Object, commandRepository.Object);




            //var repo= fixture.Freeze<Mock<IQueryRepository>>();
            // fixture.Create<Mock<IPersonRepository>>();

        }

        [Fact]
        public async void GET_retrieves_all_roles()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            IList<Role> users = new List<Role>
            {

            };


            var queryRepository = fixture.Freeze<Mock<IQueryRepository>>();
            var commandRepository = fixture.Freeze<Mock<ICommandRepository>>();
            var roles = fixture.CreateMany<Role>().AsAsyncQueryable();
            //var dbSet = GetQueryableMockDbSet<Role>(sss.ToList());


            //var mockSet = new Mock<DbSet<Role>>();
            //mockSet.As<IQueryable<Role>>().Setup(m => m.Provider).Returns(sss.Provider);
            //mockSet.As<IQueryable<Role>>().Setup(m => m.Expression).Returns(sss.Expression);
            //mockSet.As<IQueryable<Role>>().Setup(m => m.ElementType).Returns(sss.ElementType);
            //mockSet.As<IQueryable<Role>>().Setup(m => m.GetEnumerator()).Returns(() => sss.GetEnumerator());

            queryRepository.Setup(r => r.Table<Role>()).Returns(roles);

            //roleService = fixture.Create<RoleService>();
           var roleService = new RoleService(queryRepository.Object, commandRepository.Object);


            //Act
            var resultList = await roleService.GetList();

            //Assert
            resultList.Should().BeOfType<List<RoleModel>>();
        }


    }

}

