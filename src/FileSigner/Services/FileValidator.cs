using FileSigner.Models;

namespace FileSigner.Services;

/// <summary>
/// Validates file extensions and size boundaries against configured policy rules.
/// </summary>
public sealed class FileValidator : IFileValidator
{
    private readonly FileValidationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileValidator"/> class with default or specified options.
    /// </summary>
    /// <param name="options">Validation options. If null, default values are used.</param>
    public FileValidator(FileValidationOptions? options = null)
    {
        _options = options ?? new FileValidationOptions();
    }

    /// <inheritdoc />
    public FileValidationOptions Options => _options;

    /// <inheritdoc />
    public FileValidationResult Validate(string? fileName, long sizeInBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return FileValidationResult.Failure("File name cannot be empty or whitespace.");
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
        {
            return FileValidationResult.Failure($"File '{fileName}' does not contain an extension.");
        }

        if (!_options.AllowedExtensions.Contains(extension))
        {
            var allowedList = string.Join(", ", _options.AllowedExtensions);
            return FileValidationResult.Failure(
                $"Extension '{extension}' is not permitted. Allowed extensions are: {allowedList}.");
        }

        if (sizeInBytes <= 0)
        {
            return FileValidationResult.Failure("File content cannot be empty (0 bytes).");
        }

        if (sizeInBytes > _options.MaxSizeBytes)
        {
            var maxMb = _options.MaxSizeBytes / (1024.0 * 1024.0);
            var fileMb = sizeInBytes / (1024.0 * 1024.0);
            return FileValidationResult.Failure(
                $"File size ({fileMb:F2} MB, {sizeInBytes:N0} bytes) exceeds maximum permitted limit of {maxMb:F2} MB ({_options.MaxSizeBytes:N0} bytes).");
        }

        return FileValidationResult.Success();
    }

    /// <inheritdoc />
    public FileValidationResult ValidateFilePath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return FileValidationResult.Failure("File path cannot be empty or whitespace.");
        }

        if (!File.Exists(filePath))
        {
            return FileValidationResult.Failure($"File not found at path: '{filePath}'.");
        }

        var fileInfo = new FileInfo(filePath);
        return Validate(fileInfo.Name, fileInfo.Length);
    }
}
