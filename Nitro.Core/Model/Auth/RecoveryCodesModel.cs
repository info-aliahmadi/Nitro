using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nitro.Core.Model.Auth
{
    public class RecoveryCodesModel
    {
        [Required]
        public IEnumerable<string> Codes { get; set; }
    }
}
