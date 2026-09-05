using FileSigner.Models;

namespace FileSigner.Services;

/// <summary>
/// High-level application service that coordinates validation, binary conversion, in-memory persistence, and cryptographic signing.
/// </summary>
public interface IFileProcessingService
{
    /// <summary>
    /// Ingests a file from disk, validates it, reads bytes, cryptographically signs it, and stores it in memory.
    /// </summary>
    /// <param name="filePath">Path to the physical file on disk.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple indicating success status, optional error message, and the stored file descriptor if successful.</returns>
    Task<(bool Success, string? ErrorMessage, StoredFile? File)> ProcessAndStoreFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ingests raw file bytes with a virtual file name, validates size and extension, cryptographically signs it, and stores it in memory.
    /// </summary>
    /// <param name="fileName">Name of the file including extension.</param>
    /// <param name="data">The raw binary payload.</param>
    /// <returns>A tuple indicating success status, optional error message, and the stored file descriptor if successful.</returns>
    (bool Success, string? ErrorMessage, StoredFile? File) ProcessAndStoreBytes(string fileName, byte[] data);

    /// <summary>
    /// Retrieves a file from in-memory storage by its identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the stored file.</param>
    /// <returns>The stored file if present; otherwise, null.</returns>
    StoredFile? RetrieveById(Guid id);

    /// <summary>
    /// Retrieves a file from in-memory storage by its file name.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The stored file if present; otherwise, null.</returns>
    StoredFile? RetrieveByName(string fileName);

    /// <summary>
    /// Verifies the cryptographic signature of a stored file against its payload and public key.
    /// </summary>
    /// <param name="id">Unique identifier of the stored file.</param>
    /// <returns>The signature verification result.</returns>
    SignatureVerificationResult VerifyStoredFile(Guid id);

    /// <summary>
    /// Exports a stored file's binary content to a specified destination path on disk.
    /// </summary>
    /// <param name="id">Unique identifier of the stored file.</param>
    /// <param name="destinationPath">Target path on disk.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if exported successfully; otherwise, false.</returns>
    Task<bool> ExportFileAsync(Guid id, string destinationPath, CancellationToken cancellationToken = default);
}
