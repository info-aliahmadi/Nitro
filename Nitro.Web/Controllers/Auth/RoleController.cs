using Microsoft.AspNetCore.Mvc;
using Nitro.Core.Interfaces.Auth;
using Nitro.Core.Models.Auth;

namespace Nitro.Web.Controllers
{
    [ApiController]
    [Route("Api/Cms/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        private readonly ILogger<RoleController> _logger;

        public RoleController(ILogger<RoleController> logger, IRoleService roleService)
        {
            _logger = logger;
            _roleService = roleService;
        }

        [HttpGet(nameof(GetRoles))]
        public async Task<ActionResult<IEnumerable<RoleModel>>> GetRoles()
        {
            var roles = await _roleService.GetList();
            return Ok(roles);
        }

        [HttpGet(nameof(GetRole))]
        public async Task<ActionResult> GetRole(int roleId)
        {
            if (roleId <= 0)
            {
                return NotFound();
            }
            var role = await _roleService.GetById(roleId);
            return Ok(role);
        }

        [HttpPost(nameof(Add))]
        public async Task<ActionResult> Add(RoleModel roleModel)
        {
            var role = await _roleService.Add(roleModel);
            return Ok(role);
        }

        [HttpPost(nameof(Update))]
        public async Task<ActionResult<RoleModel>> Update(RoleModel roleModel)
        {
            var role = await _roleService.Update(roleModel);
            return Ok(role);
        }

        [HttpGet(nameof(Delete))]
        public async Task<ActionResult<bool>> Delete(int roleId)
        {
            var isDeleted = await _roleService.Delete(roleId);
            return Ok(isDeleted);
        }
    }
}