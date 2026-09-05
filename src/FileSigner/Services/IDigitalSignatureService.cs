using FileSigner.Models;

namespace FileSigner.Services;

/// <summary>
/// Contract for cryptographic hashing, digital signing, and signature verification.
/// </summary>
public interface IDigitalSignatureService : IDisposable
{
    /// <summary>
    /// Computes the SHA-256 hash digest of the provided byte array.
    /// </summary>
    /// <param name="data">The raw data bytes.</param>
    /// <returns>A 32-byte SHA-256 digest.</returns>
    byte[] ComputeSha256Hash(byte[] data);

    /// <summary>
    /// Signs the input data using SHA-256 and asymmetric cryptography (ECDSA P-256).
    /// </summary>
    /// <param name="data">The raw data bytes to sign.</param>
    /// <returns>A <see cref="CryptographicSignature"/> containing the hash, signature, and public key.</returns>
    CryptographicSignature Sign(byte[] data);

    /// <summary>
    /// Cryptographically verifies that the provided signature matches the data and public key.
    /// </summary>
    /// <param name="data">The raw content bytes.</param>
    /// <param name="signature">The digital signature bytes.</param>
    /// <param name="publicKey">The SubjectPublicKeyInfo DER-encoded public key bytes.</param>
    /// <returns>A <see cref="SignatureVerificationResult"/> indicating success or failure reason.</returns>
    SignatureVerificationResult Verify(byte[] data, byte[] signature, byte[] publicKey);

    /// <summary>
    /// Exports the current signer's SubjectPublicKeyInfo DER-encoded public key.
    /// </summary>
    /// <returns>The public key bytes.</returns>
    byte[] GetPublicKey();
}
