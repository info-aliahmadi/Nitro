using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Nitro.Kernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Nitro.Infrastructure.Data
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected ApplicationDbContext context;
        internal DbSet<T> dbSet;
        public readonly ILogger _logger;

        public Repository(
            ApplicationDbContext context,
            ILogger logger)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
            _logger = logger;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            CancellationToken cancellationToken = default)
        {
            IDbContextTransaction dbContextTransaction = await context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
            return dbContextTransaction;
        }

        public async Task<List<T>> GetListAsync<T>(CancellationToken cancellationToken = default) where T : class
        {
            return await GetListAsync<T>(false, cancellationToken);
        }

        public async Task<List<T>> GetListAsync<T>(bool asNoTracking, CancellationToken cancellationToken = default) where T : class
        {
            IQueryable<T> query = context.Set<T>();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            List<T> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            return items;
        }

        public async Task<T> GetByIdAsync<T>(object id, CancellationToken cancellationToken = default) where T : class
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetByIdAsync<T>(id, false, cancellationToken);
        }

        public async Task<T> GetByIdAsync<T>(object id, bool asNoTracking, CancellationToken cancellationToken = default) where T : class
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

          
            IQueryable<T> query = context.Set<T>();


            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            T enity = await query.FirstOrDefaultAsync(expressionTree, cancellationToken).ConfigureAwait(false);
            return enity;
        }

        public async Task<object[]> InsertAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync<T>(object id, CancellationToken cancellationToken = default) where T : class
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task<List<T1>> GetFromRawSqlAsync<T1>(string sql, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<List<T1>> GetFromRawSqlAsync<T1>(string sql, object parameter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<List<T1>> GetFromRawSqlAsync<T1>(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<int> ExecuteSqlCommandAsync(string sql, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<int> ExecuteSqlCommandAsync(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets a table
        /// </summary>
        public virtual IQueryable<T> Table<T>() where T : class => context.Set<T>();
    }
}
