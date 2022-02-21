

using Nitro.FileStorage.Infrastructure.SignatureVerify.FileTypes;

namespace Nitro.FileStorage.Infrastructure.SignatureVerify
{
    public class FileTypeVerifier : IFileTypeVerifier
    {
        public FileTypeVerifier()
        {

        }
        private static FileTypeVerifyResult Unknown = new FileTypeVerifyResult
        {
            Name = "Unknown",
            Description = "Unknown File Type",
            IsVerified = false
        };

        private async Task<byte[]> GetFirstBytes(Stream file)
        {
            const int chunkSize = 64;
            var instanceBuffer = new byte[chunkSize];
            await file.ReadAsync(instanceBuffer);
            return instanceBuffer;
        }

        public async Task<FileTypeVerifyResult> VerifyAsync(Stream file, string extension)
        {
            FileType fileType = FileTypeClass(extension);
            var bytes = await GetFirstBytes(file);
            var result = fileType.Verify(bytes);

            return result?.IsVerified == true
                   ? result
                   : Unknown;
        }

        private FileType FileTypeClass(string extension)
        {
            extension = extension.ToLowerInvariant();
            FileType fileType;
            switch (extension)
            {
                case ".jpg":
                    fileType = new Jpg();
                    return fileType;
                case ".png":
                    fileType = new Png();
                    return fileType;
                case ".jpeg":
                    fileType = new Jpeg();
                    return fileType;
                case ".bmp":
                    fileType = new Bmp();
                    return fileType;
                case ".doc":
                    fileType = new Doc();
                    return fileType;
                case ".docx":
                    fileType = new Docx();
                    return fileType;
                case ".gif":
                    fileType = new Gif();
                    return fileType;
                case ".mp3":
                    fileType = new Mp3();
                    return fileType;
                case ".pdf":
                    fileType = new Pdf();
                    return fileType;
                case ".txt":
                    fileType = new Txt();
                    return fileType;
                case ".rar":
                    fileType = new Rar();
                    return fileType;
                case ".tif":
                    fileType = new Tif();
                    return fileType;
                case ".tiff":
                    fileType = new Tiff();
                    return fileType;
                case ".xls":
                    fileType = new Xls();
                    return fileType;
                case ".xlsx":
                    fileType = new Xlsx();
                    return fileType;
                case ".xml":
                    fileType = new Xml();
                    return fileType;
                case ".zip":
                    fileType = new Zip();
                    return fileType;
                default:
                    break;
            }

            Exception exception = new Exception("");
            throw exception;
        }
    }
}