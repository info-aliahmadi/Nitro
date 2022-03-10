﻿
using Nitro.Kernel.Models;

namespace Nitro.Core.Model.Auth
{
    public class SendCodeModel
    {
        public string SelectedProvider { get; set; }

        public ICollection<SelectListItem> Providers { get; set; }
        
    }
}
