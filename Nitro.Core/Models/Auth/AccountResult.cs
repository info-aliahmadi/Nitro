using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nitro.Core.Models.Auth
{
    public class AccountResult
    {
        public bool Succeeded => Status == AccountStatusEnum.Succeeded;

        public AccountStatusEnum Status { get; set; }

        public string StatusDescription => Status.Description();
        public string Message { get; set; }

        public List<string> Errors { get; set; }
    }
}
