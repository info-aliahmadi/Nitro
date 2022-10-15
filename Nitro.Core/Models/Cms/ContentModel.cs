using Nitro.Core.Data.Domain;

namespace Nitro.Core.Models.Cms
{
    public record ContentModel
    {

        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int AuthorId { get; set; }


    }

}
