using FileSigner.Models;

namespace FileSigner.Services;

/// <summary>
/// Contract for validating file names, extensions, and size limits.
/// </summary>
public interface IFileValidator
{
    /// <summary>
    /// Gets the current validation configuration options.
    /// </summary>
    FileValidationOptions Options { get; }

    /// <summary>
    /// Validates a file by its name and byte size.
    /// </summary>
    /// <param name="fileName">The file name or path.</param>
    /// <param name="sizeInBytes">The size of the file in bytes.</param>
    /// <returns>A validation result indicating validity or failure reason.</returns>
    FileValidationResult Validate(string? fileName, long sizeInBytes);

    /// <summary>
    /// Validates an existing file on the local filesystem.
    /// </summary>
    /// <param name="filePath">Path to the physical file on disk.</param>
    /// <returns>A validation result indicating validity or failure reason.</returns>
    FileValidationResult ValidateFilePath(string? filePath);
}
