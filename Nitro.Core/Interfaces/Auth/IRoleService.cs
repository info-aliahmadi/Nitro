using Nitro.Core.Models.Auth;
using Nitro.Core.Models.Cms;

namespace Nitro.Core.Interfaces.Auth
{
    public interface IRoleService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<List<RoleModel>> GetList();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<RoleModel> GetById(int id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleModel"></param>
        /// <returns></returns>
        Task<RoleModel> Add(RoleModel roleModel);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleModel"></param>
        /// <returns></returns>
        Task<RoleModel> Update(RoleModel roleModel);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> Delete(int id);


    }
}
