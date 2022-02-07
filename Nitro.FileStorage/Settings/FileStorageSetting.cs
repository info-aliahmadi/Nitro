namespace Nitro.FileStorage.Models
{
    public class FileStorageSetting
    {
        public string WhiteListExtensions { get; set; } = null!;

        public long MaxSizeLimit { get; set; } = 0!;

        public long MinSizeLimit { get; set; } = 0!;
    }
}
