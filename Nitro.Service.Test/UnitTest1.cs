using FluentAssertions;
using Nitro.Core.Interfaces.Cms;
using Nitro.Infrastructure.Test;
using Xunit;

namespace Nitro.Service.Test
{
    public class UnitTest1 : IClassFixture<ApiWebApplicationFactory>
    {
        readonly IAuthorService _authorService;
        public UnitTest1(IAuthorService authorService)
        {
            _authorService = authorService;
        }
        [Fact]
        public async void Test1()
        {
            var result = await _authorService.GetList();

            result.Should().BeEmpty();


        }
    }
}