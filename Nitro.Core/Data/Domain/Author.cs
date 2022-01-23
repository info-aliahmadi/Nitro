using Nitro.Infrastructure.Data;
using Nitro.Infrastructure.Data.IdentityDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nitro.Core.Data.Domain
{
    public class Author : BaseEntity
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
