

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


        public FileTypeVerifyResult Verify(Stream file, string extension)
        {
            if (true)
            {

            }
            FileType fileType = FileTypeClass(extension);

            var result = fileType.Verify(file);

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
                case "jpg":
                    fileType = new Jpeg();
                    return fileType;
                case "png":
                    fileType = new Png();
                    return fileType;
                case "jpeg":
                    fileType = new Jpeg();
                    return fileType;
                default:
                    break;
            }

            Exception exception = new Exception("");
            throw exception;
        }
    }
}