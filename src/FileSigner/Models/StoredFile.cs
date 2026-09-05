namespace FileSigner.Models;

/// <summary>
/// Represents a file stored in memory along with its cryptographic hash, digital signature, and metadata.
/// </summary>
/// <param name="Id">Unique identifier of the stored file.</param>
/// <param name="FileName">Original name of the file including extension.</param>
/// <param name="Extension">File extension in lowercase with leading dot.</param>
/// <param name="SizeInBytes">Size of the file in bytes.</param>
/// <param name="Data">Raw binary content of the file.</param>
/// <param name="Sha256Hash">Cryptographic SHA-256 hash of the binary content.</param>
/// <param name="Signature">ECDSA digital signature of the hash.</param>
/// <param name="PublicKey">SubjectPublicKeyInfo DER-encoded public key used to verify the signature.</param>
/// <param name="CreatedAt">Timestamp indicating when the file was ingested and signed.</param>
public sealed record StoredFile(
    Guid Id,
    string FileName,
    string Extension,
    long SizeInBytes,
    byte[] Data,
    byte[] Sha256Hash,
    byte[] Signature,
    byte[] PublicKey,
    DateTimeOffset CreatedAt);
