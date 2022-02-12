namespace Nitro.FileStorage.Infrastructure.Settings
{
    public interface IFileStorageDatabaseSetting
    {
        string ConnectionString { get; set; }

        string DatabaseName { get; set; }

    }
}
