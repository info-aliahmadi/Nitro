using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nitro.Core.Models.Auth
{
    public record ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

}
