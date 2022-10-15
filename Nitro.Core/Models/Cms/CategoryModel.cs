using Nitro.Kernel;

namespace Nitro.Core.Models.Cms
{
    public record CategoryModel 
    {

        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; }
    }
}