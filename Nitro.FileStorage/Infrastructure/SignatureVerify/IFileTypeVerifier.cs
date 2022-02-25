

using Nitro.FileStorage.Infrastructure.SignatureVerify.FileTypes;

namespace Nitro.FileStorage.Infrastructure.SignatureVerify
{
    public interface IFileTypeVerifier
    {
        Task<FileTypeVerifyResult> VerifyAsync(byte[] file, string extension);
    }
}