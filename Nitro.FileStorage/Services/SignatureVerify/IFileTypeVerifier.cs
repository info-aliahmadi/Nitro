using Nitro.FileStorage.Services.SignatureVerify.FileTypes;

namespace Nitro.FileStorage.Services.SignatureVerify
{
    public interface IFileTypeVerifier
    {
        FileTypeVerifyResult Verify(Stream file, string extension);
    }
}