

using Nitro.FileStorage.Infrastructure.SignatureVerify.FileTypes;

namespace Nitro.FileStorage.Infrastructure.SignatureVerify
{
    public interface IFileTypeVerifier
    {
        FileTypeVerifyResult Verify(Stream file, string extension);
    }
}