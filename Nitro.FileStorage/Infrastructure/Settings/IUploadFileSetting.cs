﻿namespace Nitro.FileStorage.Infrastructure.Settings
{
    public interface IUploadFileSetting
    {
        string WhiteListExtensions { get; set; } 
        string SignatureValidationExtensions { get; set; }

        long MaxSizeLimitSmallFile { get; set; }

        long MinSizeLimitSmallFile { get; set; }

        long MaxSizeLimitLargeFile { get; set; }

        long MinSizeLimitLargeFile { get; set; }
    }
}
