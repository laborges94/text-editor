using FileSigner.Models;

namespace FileSigner.Services;

/// <summary>
/// Contract for storing and retrieving files and cryptographic metadata in memory.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Gets the number of files currently stored in memory.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the total size in bytes of all file payloads currently residing in memory.
    /// </summary>
    long TotalMemoryBytes { get; }

    /// <summary>
    /// Stores a file payload along with its cryptographic signature and metadata in memory.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="data">The raw binary payload.</param>
    /// <param name="signature">The cryptographic signature details.</param>
    /// <returns>The newly created <see cref="StoredFile"/> descriptor.</returns>
    StoredFile Store(string fileName, byte[] data, CryptographicSignature signature);

    /// <summary>
    /// Retrieves a stored file by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the stored file.</param>
    /// <returns>The stored file if found; otherwise, null.</returns>
    StoredFile? GetById(Guid id);

    /// <summary>
    /// Retrieves a stored file by its original file name (case-insensitive search).
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The stored file if found; otherwise, null.</returns>
    StoredFile? GetByFileName(string fileName);

    /// <summary>
    /// Retrieves all files currently stored in memory.
    /// </summary>
    /// <returns>A read-only collection of all stored files.</returns>
    IReadOnlyCollection<StoredFile> GetAll();

    /// <summary>
    /// Removes a stored file from memory by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the file to remove.</param>
    /// <returns>True if the file was found and removed; otherwise, false.</returns>
    bool Delete(Guid id);

    /// <summary>
    /// Clears all stored files from memory.
    /// </summary>
    void Clear();
}
