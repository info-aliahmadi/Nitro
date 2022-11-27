using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Nitro.Core.Models.Auth;
using Nitro.Infrastructure.Test;
using Nitro.Web.Controllers;
using System.Collections.Generic;
using Xunit;

namespace Nitro.Web.Test.Auth
{
    public class RoleControllerTest : IClassFixture<ApiWebApplicationFactory>
    {
        readonly Fixture fixture;
        readonly RoleController roleController;
        public RoleControllerTest()
        {
            //Arrange
            fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization());
            roleController = fixture.Build<RoleController>().OmitAutoProperties().Create();
        }

        [Fact]
        public async void GET_retrieves_roles()
        {
            //Act
            var resultList = await roleController.GetRoles();

            //Assert
            resultList.Should().BeOfType<ActionResult<IEnumerable<RoleModel>>>();
        }
        [Theory]
        [InlineData(0)]
        public async void GET_retrieve_role_by_zero(int id)
        {
            //Act
            var resultList = await roleController.GetRole(id);

            //Assert
            resultList.Should().BeOfType<NotFoundResult>();
        }
        [Theory]
        [InlineData(1)]
        public async void GET_retrieve_role_by_id(int id)
        {
            //Act
            var resultList = await roleController.GetRole(id);

            //Assert
            resultList.Should().BeOfType<OkObjectResult>();
        }
        [Theory, AutoData]
        public async void POST_add_role(RoleModel roleModel)
        {
            //Act
            var resultList = await roleController.Add(roleModel);

            //Assert
            resultList.Should().BeOfType<OkObjectResult>();
        }
        [Theory, AutoData]
        public async void POST_update_role(RoleModel roleModel)
        {
            //Act
            var resultList = await roleController.Update(roleModel);

            //Assert
            resultList.Should().BeOfType<ActionResult<RoleModel>>();
        }
        [Theory, AutoData]
        public async void POST_delete_role(int roleId)
        {
            //Act
            var resultList = await roleController.Delete(roleId);

            //Assert
            resultList.Should().BeOfType<ActionResult<bool>>();
        }
    }
}
