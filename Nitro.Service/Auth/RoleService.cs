﻿using Microsoft.EntityFrameworkCore;
using Nitro.Core.Data.Domain;
using Nitro.Core.Domain.Auth;
using Nitro.Core.Interfaces.Auth;
using Nitro.Core.Interfaces.Cms;
using Nitro.Core.Models.Auth;
using Nitro.Core.Models.Cms;
using Nitro.Kernel.Interfaces.Data;

namespace Nitro.Service.Cms
{
    public class RoleService : IRoleService
    {
        private readonly IQueryRepository _queryRepository;
        private readonly ICommandRepository _commandRepository;

        public RoleService(IQueryRepository queryRepository, ICommandRepository commandRepository)
        {
            _queryRepository = queryRepository;
            _commandRepository = commandRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<RoleModel>> GetList()
        {
            var list = await _queryRepository.Table<Role>().ToListAsync();
            
           var result = list.Select(x => new RoleModel()
            {
                Id = x.Id,
                Name = x.Name,
                ConcurrencyStamp = x.ConcurrencyStamp,
                NormalizedName = x.NormalizedName
            }).ToList();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RoleModel> GetById(int id)
        {
            var record = await _queryRepository.Table<Role>().Where(x => x.Id == id)
                .FirstOrDefaultAsync();
            var role = new RoleModel();
            if (record !=null)
            {
                role.Id = record!.Id;
                role.Name = record.Name;
                role.ConcurrencyStamp = record.ConcurrencyStamp;
                role.NormalizedName = record.NormalizedName;
            }

            return role;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleModel"></param>
        /// <returns></returns>
        public async Task<RoleModel> Add(RoleModel roleModel)
        {
            var role = new Role()
            {
                Name = roleModel.Name,
                ConcurrencyStamp = roleModel.ConcurrencyStamp,
                NormalizedName = roleModel.NormalizedName
            };
            await _commandRepository.InsertAsync(role);

            roleModel.Id = role.Id;

            return roleModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleModel"></param>
        /// <returns></returns>
        public async Task<RoleModel> Update(RoleModel roleModel)
        {
            var role = await _queryRepository.Table<Role>().FirstOrDefaultAsync(x => x.Id == roleModel.Id);
            if (role is null)
            {
                throw new Exception("Not Found");
            }

            role.Name = roleModel.Name;
            role.ConcurrencyStamp = roleModel.ConcurrencyStamp;
            role.NormalizedName = roleModel.NormalizedName;

            _commandRepository.UpdateAsync(role);

            return roleModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Delete(int id)
        {
            var role = await _queryRepository.Table<Role>().FirstOrDefaultAsync(x => x.Id == id);
            if (role == null)
            {
                return false;
            }

            _commandRepository.DeleteAsync(role);

            return true;
        }
    }
}