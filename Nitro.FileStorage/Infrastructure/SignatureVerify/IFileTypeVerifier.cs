

using Nitro.FileStorage.Infrastructure.SignatureVerify.FileTypes;

namespace Nitro.FileStorage.Infrastructure.SignatureVerify
{
    public interface IFileTypeVerifier
    {
        FileTypeVerifyResult Verify(byte[] file, string extension);
    }
}