﻿// <copyright file="QueryRepository.cs" company="TanvirArjel">
// Copyright (c) TanvirArjel. All rights reserved.
// </copyright>

using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq.Expressions;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Nitro.Infrastructure.Data.Extension;
using Nitro.Kernel;
using Nitro.Kernel.Extensions;
using Nitro.Kernel.Interfaces;

namespace Nitro.Infrastructure.Data
{
    internal class QueryRepository : IQueryRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public QueryRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<T> Table<T>() where T : class => _dbContext.Set<T>();
 

        public Task<List<T>> GetListAsync<T>(bool cacheable = false,CancellationToken cancellationToken = default)
            where T : class
        {
            return GetListAsync<T>(asNoTracking: false,cacheable: cacheable, cancellationToken);
        }

        public Task<List<T>> GetListAsync<T>(bool asNoTracking, bool cacheable = false, CancellationToken cancellationToken = default)
            where T : class
        {
            Func<IQueryable<T>, IIncludableQueryable<T, object>> nullValue = null;
            return GetListAsync(nullValue, asNoTracking, cacheable: cacheable, cancellationToken);
        }

        public Task<List<T>> GetListAsync<T>(
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
             bool cacheable = false,
            CancellationToken cancellationToken = default)
            where T : class
        {
            return GetListAsync(includes, false, cacheable: cacheable, cancellationToken);
        }

        public async Task<List<T>> GetListAsync<T>(
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
            bool asNoTracking,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
            where T : class
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (includes != null)
            {
                query = includes(query);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (cacheable)
            {
                query = query.Cacheable();
            }

            List<T> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            return items;
        }

        public Task<List<T>> GetListAsync<T>(
            Expression<Func<T, bool>> condition,
            bool cacheable = false, 
            CancellationToken cancellationToken = default)
             where T : class
        {
            return GetListAsync(condition,false, cacheable, cancellationToken);
        }

        public Task<List<T>> GetListAsync<T>(
            Expression<Func<T, bool>> condition,
            bool asNoTracking,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
             where T : class
        {
            return GetListAsync(condition, null, asNoTracking, cacheable, cancellationToken);
        }

        public async Task<List<T>> GetListAsync<T>(
            Expression<Func<T, bool>> condition,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
            bool asNoTracking,
            bool cacheable =false,
            CancellationToken cancellationToken = default)
             where T : class
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (condition != null)
            {
                query = query.Where(condition);
            }

            if (includes != null)
            {
                query = includes(query);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (cacheable)
            {
                query = query.Cacheable();
            }

            List<T> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            return items;
        }

        public Task<List<T>> GetListAsync<T>(Specification<T> specification, bool cacheable = false, CancellationToken cancellationToken = default)
           where T : class
        {
            return GetListAsync(specification, false, cacheable, cancellationToken);
        }

        public async Task<List<T>> GetListAsync<T>(
            Specification<T> specification,
            bool asNoTracking,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
           where T : class
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (specification != null)
            {
                query = query.GetSpecifiedQuery(specification);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (cacheable)
            {
                query = query.Cacheable();
            }

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<TProjectedType>> GetListAsync<T, TProjectedType>(
            Expression<Func<T, TProjectedType>> selectExpression,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
            where T : class
        {
            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }
            IQueryable<T> query = _dbContext.Set<T>();

            if (cacheable)
            {
                query = query.Cacheable();
            }

            List<TProjectedType> entities = await query
                .Select(selectExpression).ToListAsync(cancellationToken).ConfigureAwait(false);

            return entities;
        }

        public async Task<List<TProjectedType>> GetListAsync<T, TProjectedType>(
            Expression<Func<T, bool>> condition,
            Expression<Func<T, TProjectedType>> selectExpression,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
            where T : class
        {
            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }

            IQueryable<T> query = _dbContext.Set<T>();

            if (condition != null)
            {
                query = query.Where(condition);
            }
            if (cacheable)
            {
                query = query.Cacheable();
            }


            List<TProjectedType> projectedEntites = await query.Select(selectExpression)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            return projectedEntites;
        }

        public async Task<List<TProjectedType>> GetListAsync<T, TProjectedType>(
            Specification<T> specification,
            Expression<Func<T, TProjectedType>> selectExpression,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
            where T : class
        {
            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }

            IQueryable<T> query = _dbContext.Set<T>();

            if (specification != null)
            {
                query = query.GetSpecifiedQuery(specification);
            }
            if (cacheable)
            {
                query = query.Cacheable();
            }

            return await query.Select(selectExpression)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<PaginatedList<T>> GetListAsync<T>(
            PaginationSpecification<T> specification,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
            where T : class
        {
            if (specification == null)
            {
                throw new ArgumentNullException(nameof(specification));
            }

            IQueryable<T> query = _dbContext.Set<T>();

            if (cacheable)
            {
                query = query.Cacheable();
            }

            PaginatedList<T> paginatedList = await query.ToPaginatedListAsync(specification, cancellationToken);
            return paginatedList;
        }

        public async Task<PaginatedList<TProjectedType>> GetListAsync<T, TProjectedType>(
            PaginationSpecification<T> specification,
            Expression<Func<T, TProjectedType>> selectExpression,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
            where T : class
            where TProjectedType : class
        {
            if (specification == null)
            {
                throw new ArgumentNullException(nameof(specification));
            }

            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }

            IQueryable<T> query = _dbContext.Set<T>().GetSpecifiedQuery((SpecificationBase<T>)specification);

            if (cacheable)
            {
                query = query.Cacheable();
            }

            PaginatedList<TProjectedType> paginatedList = await query.Select(selectExpression)
                .ToPaginatedListAsync(specification.PageIndex, specification.PageSize, cancellationToken);
            return paginatedList;
        }
        public Task<T> GetByIdAsync<T>(object id, bool cacheable=false, CancellationToken cancellationToken = default)
                  where T : BaseEntity<object>
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetByIdAsync<T>(id, asNoTracking:false, cacheable: cacheable, cancellationToken);
        }

        public Task<T> GetByIdAsync<T>(object id, 
            bool asNoTracking = false,
            bool cacheable = false, 
            CancellationToken cancellationToken = default)
            where T : BaseEntity<object>
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetByIdAsync<T>(id, null, asNoTracking, cacheable: cacheable, cancellationToken);
        }

        public Task<T> GetByIdAsync<T>(
            object id,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
            where T : BaseEntity<object>
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetByIdAsync(id, includes, asNoTracking: false, cacheable:cacheable, cancellationToken);
        }

        public async Task<T> GetByIdAsync<T>(
            object id,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
            bool asNoTracking = false,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
            where T : BaseEntity<object>
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            IQueryable<T> query = _dbContext.Set<T>();

            if (includes != null)
            {
                query = includes(query);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            if (cacheable)
            {
                query = query.Cacheable();
            }

            T? enity = await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
            if (enity == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            return enity;
        }

        public async Task<TProjectedType> GetByIdAsync<T, TProjectedType>(
            object id,
            Expression<Func<T, TProjectedType>> selectExpression,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
            where T : BaseEntity<object>
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }
            IQueryable<T> query = _dbContext.Set<T>();

            
            if (cacheable)
            {
                query = query.Cacheable();
            }
            TProjectedType? enity = await query.Where(x => x.Id == id).Select(selectExpression)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            if (enity == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            return enity;
        }

        public Task<T> GetAsync<T>(
            Expression<Func<T, bool>> condition,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
           where T : class
        {
            return GetAsync(condition, null, false, cancellationToken);
        }

        public Task<T> GetAsync<T>(
            Expression<Func<T, bool>> condition,
            bool asNoTracking,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
           where T : class
        {
            return GetAsync(condition, null, asNoTracking, cacheable, cancellationToken);
        }

        public Task<T> GetAsync<T>(
            Expression<Func<T, bool>> condition,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
           where T : class
        {
            return GetAsync(condition, includes, false, cacheable, cancellationToken);
        }

        public async Task<T> GetAsync<T>(
            Expression<Func<T, bool>> condition,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
            bool asNoTracking,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
           where T : class
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (condition != null)
            {
                query = query.Where(condition);
            }

            if (includes != null)
            {
                query = includes(query);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task<T> GetAsync<T>(Specification<T> specification,
            bool cacheable = false, CancellationToken cancellationToken = default)
            where T : class
        {
            return GetAsync(specification, false, cacheable, cancellationToken);
        }

        public async Task<T> GetAsync<T>(Specification<T> specification, bool asNoTracking, bool cacheable = false, CancellationToken cancellationToken = default)
            where T : class
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (specification != null)
            {
                query = query.GetSpecifiedQuery(specification);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            if (cacheable)
            {
                query = query.Cacheable();
            }

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TProjectedType> GetAsync<T, TProjectedType>(
            Expression<Func<T, bool>> condition,
            Expression<Func<T, TProjectedType>> selectExpression,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
            where T : class
        {
            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }

            IQueryable<T> query = _dbContext.Set<T>();

            if (condition != null)
            {
                query = query.Where(condition);
            }
            if (cacheable)
            {
                query = query.Cacheable();
            }

            return await query.Select(selectExpression).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TProjectedType> GetAsync<T, TProjectedType>(
            Specification<T> specification,
            Expression<Func<T, TProjectedType>> selectExpression,
            bool cacheable = false,
            CancellationToken cancellationToken = default)
            where T : class
        {
            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }

            IQueryable<T> query = _dbContext.Set<T>();

            if (specification != null)
            {
                query = query.GetSpecifiedQuery(specification);
            }

            if (cacheable)
            {
                query = query.Cacheable();
            }
            return await query.Select(selectExpression).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task<bool> ExistsAsync<T>(CancellationToken cancellationToken = default)
           where T : BaseEntity<object>
        {
            return ExistsAsync<T>(null, cancellationToken);
        }

        public async Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cancellationToken = default)
           where T : BaseEntity<object>
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (condition == null)
            {
                return await query.AnyAsync(cancellationToken);
            }

            bool isExists = await query.AnyAsync(condition, cancellationToken).ConfigureAwait(false);
            return isExists;
        }

        public async Task<bool> ExistsByIdAsync<T>(object id, CancellationToken cancellationToken = default)
           where T : BaseEntity<object>
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            IQueryable<T> query = _dbContext.Set<T>();

            bool isExistent = await query.AnyAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
            return isExistent;
        }

        public async Task<int> GetCountAsync<T>(CancellationToken cancellationToken = default)
            where T : class
        {
            int count = await _dbContext.Set<T>().CountAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }

        public async Task<int> GetCountAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cancellationToken = default)
            where T : class
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (condition != null)
            {
                query = query.Where(condition);
            }

            return await query.CountAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> GetCountAsync<T>(IEnumerable<Expression<Func<T, bool>>> conditions, CancellationToken cancellationToken = default)
            where T : class
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (conditions != null)
            {
                foreach (Expression<Func<T, bool>> expression in conditions)
                {
                    query = query.Where(expression);
                }
            }

            return await query.CountAsync(cancellationToken).ConfigureAwait(false);
        }

        // DbConext level members
        public async Task<List<T>> GetFromRawSqlAsync<T>(string sql, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            IEnumerable<object> parameters = new List<object>();

            List<T> items = await _dbContext.GetFromQueryAsync<T>(sql, parameters, cancellationToken);
            return items;
        }

        public async Task<List<T>> GetFromRawSqlAsync<T>(string sql, object parameter, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            List<object> parameters = new List<object>() { parameter };
            List<T> items = await _dbContext.GetFromQueryAsync<T>(sql, parameters, cancellationToken);
            return items;
        }

        public async Task<List<T>> GetFromRawSqlAsync<T>(string sql, IEnumerable<DbParameter> parameters, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            List<T> items = await _dbContext.GetFromQueryAsync<T>(sql, parameters, cancellationToken);
            return items;
        }

        public async Task<List<T>> GetFromRawSqlAsync<T>(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            List<T> items = await _dbContext.GetFromQueryAsync<T>(sql, parameters, cancellationToken);
            return items;
        }
    }
}
