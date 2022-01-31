﻿using Nitro.Kernel;


namespace Nitro.Core.Data.Domain
{
    public class Category : BaseEntity<int>
    {
        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<Content> Content { get; set; }
    }
}
