

using Nitro.FileStorage.Infrastructure.SignatureVerify.FileTypes;

namespace Nitro.FileStorage.Infrastructure.SignatureVerify
{
    public interface IFileTypeVerifier
    {
        Task<FileTypeVerifyResult> VerifyAsync(Stream file, string extension);
    }
}