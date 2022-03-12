using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nitro.Core.Model.Auth
{
    public class UseRecoveryCodeModel
    {
        [Required]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }
    }
}
