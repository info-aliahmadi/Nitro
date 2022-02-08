namespace Nitro.FileStorage.Models
{
    public interface IUploadFileSetting
    {
        public string WhiteListExtensions { get; set; }
        public string SignatureValidationExtensions { get; set; }

        public long MaxSizeLimitSmallFile { get; set; }

        public long MinSizeLimitSmallFile { get; set; }

        public long MaxSizeLimitLargeFile { get; set; }

        public long MinSizeLimitLargeFile { get; set; }
    }
}
