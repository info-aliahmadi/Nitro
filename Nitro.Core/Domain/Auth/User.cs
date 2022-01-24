using Microsoft.AspNetCore.Identity;

namespace Nitro.Core.Domain.Auth
{
    public class User : IdentityUser<int>
    {
        [PersonalData]
        public string Name { get; set; }

        [PersonalData]
        public DateTime DOB { get; set; }
    }
}
