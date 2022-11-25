using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nitro.Core.Interfaces.Cms;
using Nitro.Core.Models.Cms;

namespace Nitro.Web.Controllers.Cms
{
    [ApiController]
    [Route("Api/Cms/[controller]")]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        private readonly ILogger<AuthorController> _logger;

        public AuthorController(ILogger<AuthorController> logger, IAuthorService authorService)
        {
            _logger = logger;
            _authorService = authorService;
        }

        [HttpGet( nameof(GetAuthors))]
        public async Task<ActionResult<IEnumerable<AuthorModel>>> GetAuthors()
        {
            var authors = await _authorService.GetList();
            return Ok(authors);
        }

        [HttpGet(nameof(GetAuthor))]
        public async Task<ActionResult> GetAuthor(int authorId)
        {
            if (authorId <= 0)
            {
                return NotFound();
            }
            var author = await _authorService.GetById(authorId);
            return Ok(author);
        }

        [HttpPost( nameof(Add))]
        public async Task<ActionResult> Add(AuthorModel authorModel)
        {
            var author = await _authorService.Add(authorModel);
            return Ok(author);
        }

        [HttpPost( nameof(Update))]
        public async Task<ActionResult<AuthorModel>> Update(AuthorModel authorModel)
        {
            var author = await _authorService.Update(authorModel);
            return Ok(author);
        }

        [HttpGet( nameof(Delete))]
        public async Task<ActionResult<bool>> Delete(int authorId)
        {
            var isDeleted = await _authorService.Delete(authorId);
            return Ok(isDeleted);
        }
    }
}