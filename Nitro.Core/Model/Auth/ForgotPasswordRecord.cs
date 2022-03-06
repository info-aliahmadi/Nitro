﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nitro.Core.Model.Auth
{
    public record ForgotPasswordRecord
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

}
