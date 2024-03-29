﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nitro.Core.Models.Auth
{
    public enum AccountStatusEnum
    {
        [Description("Succeeded")]
        Succeeded = 0,

        [Description("Failed")]
        Failed = 1,

        [Description("Invalid")]
        Invalid = 2,

        [Description("RequiresTwoFactor")]
        RequiresTwoFactor = 3,

        [Description("IsLockedOut")]
        IsLockedOut = 4,

        [Description("IsNotAllowed")]
        IsNotAllowed = 5,

        [Description("RequireConfirmedEmail")]
        RequireConfirmedEmail = 6,

        [Description("ErrorExternalProvider")]
        ErrorExternalProvider = 7,

        [Description("NullExternalLoginInfo")]
        NullExternalLoginInfo = 8,

        [Description("ExternalLoginFailure")]
        ExternalLoginFailure = 9,

        [Description("InvalidCode")]
        InvalidCode = 10

    }
    public static class EnumExtensions
    {
        public static string Description(this AccountStatusEnum val)
        {
            DescriptionAttribute[]? attributes = (DescriptionAttribute[])val
                .GetType()
                .GetField(val.ToString())
                ?.GetCustomAttributes(typeof(DescriptionAttribute), false)!;
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}
