using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nitro.Core.Model.Auth
{
    public class RemoveLoginModel
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
    }
}
