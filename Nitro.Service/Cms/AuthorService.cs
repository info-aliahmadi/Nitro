using Microsoft.EntityFrameworkCore;
using Nitro.Core.Data.Domain;
using Nitro.Core.Interfaces.Cms;
using Nitro.Core.Models.Cms;
using Nitro.Kernel.Interfaces.Data;

namespace Nitro.Service.Cms
{
    public class AuthorService : IAuthorService
    {
        private readonly IQueryRepository _queryRepository;
        private readonly ICommandRepository _commandRepository;

        public AuthorService(IQueryRepository queryRepository, ICommandRepository commandRepository)
        {
            _queryRepository = queryRepository;
            _commandRepository = commandRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<AuthorModel>> GetList()
        {
            var result = await _queryRepository.Table<Author>().Select(x => new AuthorModel()
            {
                Id = x.Id,
                UserId = x.UserId,
                UserName = x.User.UserName,
                FirstName = x.FirstName,
                LastName = x.LastName
            }).ToListAsync();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AuthorModel> GetById(int id)
        {
            var record = await _queryRepository.Table<Author>().Include(x => x.User).Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            var author = new AuthorModel()
            {
                Id = record!.Id,
                UserId = record.UserId,
                UserName = record.User.UserName,
                FirstName = record.FirstName,
                LastName = record.LastName
            };

            return author;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authorModel"></param>
        /// <returns></returns>
        public async Task<AuthorModel> Add(AuthorModel authorModel)
        {
            var author = new Author()
            {
                UserId = authorModel.UserId,
                FirstName = authorModel.FirstName,
                LastName = authorModel.LastName
            };
            await _commandRepository.InsertAsync(author);

            authorModel.Id = author.Id;

            return authorModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authorModel"></param>
        /// <returns></returns>
        public async Task<AuthorModel> Update(AuthorModel authorModel)
        {
            var author = await _queryRepository.Table<Author>().FirstOrDefaultAsync(x => x.Id == authorModel.Id);
            if (author == null)
            {
                throw new Exception("");
            }

            author.FirstName = authorModel.FirstName;
            author.LastName = authorModel.LastName;

            _commandRepository.UpdateAsync(author);

            return authorModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Delete(int id)
        {
            var author = await _queryRepository.Table<Author>().FirstOrDefaultAsync(x => x.Id == id);
            if (author == null)
            {
                return false;
            }

            _commandRepository.DeleteAsync(author);

            return true;
        }
    }
}