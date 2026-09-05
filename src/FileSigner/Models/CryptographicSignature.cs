namespace FileSigner.Models;

/// <summary>
/// Encapsulates cryptographic signature artifacts produced during file signing.
/// </summary>
/// <param name="Sha256Hash">Cryptographic SHA-256 hash digest of the content.</param>
/// <param name="Signature">Asymmetric digital signature bytes.</param>
/// <param name="PublicKey">DER-encoded SubjectPublicKeyInfo bytes used for verification.</param>
/// <param name="Algorithm">Algorithm descriptor string (e.g. ECDSA-P256-SHA256).</param>
public sealed record CryptographicSignature(
    byte[] Sha256Hash,
    byte[] Signature,
    byte[] PublicKey,
    string Algorithm = "ECDSA-P256-SHA256")
{
    /// <summary>
    /// Gets the SHA-256 hash formatted as a lowercase hexadecimal string.
    /// </summary>
    public string Sha256Hex => Convert.ToHexString(Sha256Hash).ToLowerInvariant();

    /// <summary>
    /// Gets the digital signature formatted as a Base64 string.
    /// </summary>
    public string SignatureBase64 => Convert.ToBase64String(Signature);

    /// <summary>
    /// Gets the public key formatted as a Base64 string.
    /// </summary>
    public string PublicKeyBase64 => Convert.ToBase64String(PublicKey);
}
