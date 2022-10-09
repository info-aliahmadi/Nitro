using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Nitro.Infrastructure.Data.Extension;
using Nitro.Kernel.Interfaces;
using System.Data;
using Nitro.Kernel.Interfaces.Data;

namespace Nitro.Infrastructure.Data
{
    internal class Repository : QueryRepository, CommandRepository
    {
        private readonly IEFCacheServiceProvider _cacheService;
        private readonly ApplicationDbContext _dbContext;

        public Repository(ApplicationDbContext dbContext, IEFCacheServiceProvider cacheService)
            : base(dbContext)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
        }



    }
}