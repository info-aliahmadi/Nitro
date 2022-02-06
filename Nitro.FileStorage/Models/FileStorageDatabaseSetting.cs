namespace Nitro.FileStorage.Models
{
    public class FileStorageDatabaseSetting
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string FilesCollectionName { get; set; } = null!;
    }
}
