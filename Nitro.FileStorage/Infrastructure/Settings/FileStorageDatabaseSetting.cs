namespace Nitro.FileStorage.Infrastructure.Settings
{
    public class FileStorageDatabaseSetting : IFileStorageDatabaseSetting
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

    }
}
