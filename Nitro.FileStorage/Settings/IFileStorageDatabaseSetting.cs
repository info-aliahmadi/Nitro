namespace Nitro.FileStorage.Models
{
    public interface IFileStorageDatabaseSetting
    {
        string ConnectionString { get; set; }

        string DatabaseName { get; set; }

    }
}
