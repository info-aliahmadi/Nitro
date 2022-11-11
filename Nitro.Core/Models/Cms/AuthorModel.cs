﻿using Nitro.Core.Domain.Auth;
using Nitro.Kernel;

namespace Nitro.Core.Models.Cms
{
    public record AuthorModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? FullName => FirstName + " " + LastName;
    }
}