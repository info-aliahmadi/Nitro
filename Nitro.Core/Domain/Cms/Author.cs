﻿using Nitro.Core.Domain.Auth;
using Nitro.Kernel;

namespace Nitro.Core.Data.Domain
{
    public class Author : BaseEntity<int>
    {
        public int UserId { get; set; }
        public User User { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? FirstName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? LastName { get; set; }
    }
}
