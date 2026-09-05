using System.Security.Cryptography;
using FileSigner.Models;

namespace FileSigner.Services;

/// <summary>
/// Implements cryptographic signing and verification using SHA-256 and ECDSA with the NIST P-256 curve.
/// </summary>
public sealed class EcdsaDigitalSignatureService : IDigitalSignatureService
{
    private readonly ECDsa _ecdsa;
    private readonly byte[] _publicKeyBytes;
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="EcdsaDigitalSignatureService"/> class with a freshly generated ECDSA P-256 key pair.
    /// </summary>
    public EcdsaDigitalSignatureService()
    {
        _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _publicKeyBytes = _ecdsa.ExportSubjectPublicKeyInfo();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EcdsaDigitalSignatureService"/> class with an existing ECDSA instance.
    /// </summary>
    /// <param name="ecdsa">The initialized ECDSA instance.</param>
    public EcdsaDigitalSignatureService(ECDsa ecdsa)
    {
        ArgumentNullException.ThrowIfNull(ecdsa);
        _ecdsa = ecdsa;
        _publicKeyBytes = _ecdsa.ExportSubjectPublicKeyInfo();
    }

    /// <inheritdoc />
    public byte[] ComputeSha256Hash(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return SHA256.HashData(data);
    }

    /// <inheritdoc />
    public CryptographicSignature Sign(byte[] data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(data);

        var hash = ComputeSha256Hash(data);
        byte[] signature;

        lock (_lock)
        {
            signature = _ecdsa.SignHash(hash);
        }

        return new CryptographicSignature(hash, signature, _publicKeyBytes);
    }

    /// <inheritdoc />
    public SignatureVerificationResult Verify(byte[] data, byte[] signature, byte[] publicKey)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(signature);
        ArgumentNullException.ThrowIfNull(publicKey);

        try
        {
            using var verifier = ECDsa.Create();
            verifier.ImportSubjectPublicKeyInfo(publicKey, out _);

            var hash = ComputeSha256Hash(data);
            var isSignatureValid = verifier.VerifyHash(hash, signature);

            return isSignatureValid
                ? SignatureVerificationResult.Valid()
                : SignatureVerificationResult.Invalid("Signature verification failed: the signature does not match the payload or public key.");
        }
        catch (CryptographicException ex)
        {
            return SignatureVerificationResult.Invalid($"Cryptographic error during verification: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public byte[] GetPublicKey()
    {
        return (byte[])_publicKeyBytes.Clone();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _ecdsa.Dispose();
        _disposed = true;
    }
}
