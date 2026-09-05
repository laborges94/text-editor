using FileSigner.Models;

namespace FileSigner.Services;

/// <summary>
/// Orchestrates file ingestion, validation, cryptographic signing, in-memory persistence, and retrieval.
/// </summary>
public sealed class FileProcessingService : IFileProcessingService
{
    private readonly IFileValidator _validator;
    private readonly IDigitalSignatureService _signatureService;
    private readonly IFileStorageService _storageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileProcessingService"/> class.
    /// </summary>
    /// <param name="validator">Validator for checking extension and size limits.</param>
    /// <param name="signatureService">Service providing cryptographic hashing and digital signing.</param>
    /// <param name="storageService">In-memory file storage repository.</param>
    public FileProcessingService(
        IFileValidator validator,
        IDigitalSignatureService signatureService,
        IFileStorageService storageService)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _signatureService = signatureService ?? throw new ArgumentNullException(nameof(signatureService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    /// <inheritdoc />
    public async Task<(bool Success, string? ErrorMessage, StoredFile? File)> ProcessAndStoreFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var validation = _validator.ValidateFilePath(filePath);
        if (!validation.IsValid)
        {
            return (false, validation.ErrorMessage, null);
        }

        byte[] bytes;
        try
        {
            bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to read file from disk: {ex.Message}", null);
        }

        var fileName = Path.GetFileName(filePath);
        var signature = _signatureService.Sign(bytes);
        var storedFile = _storageService.Store(fileName, bytes, signature);

        return (true, null, storedFile);
    }

    /// <inheritdoc />
    public (bool Success, string? ErrorMessage, StoredFile? File) ProcessAndStoreBytes(string fileName, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var validation = _validator.Validate(fileName, data.LongLength);
        if (!validation.IsValid)
        {
            return (false, validation.ErrorMessage, null);
        }

        var signature = _signatureService.Sign(data);
        var storedFile = _storageService.Store(fileName, data, signature);

        return (true, null, storedFile);
    }

    /// <inheritdoc />
    public StoredFile? RetrieveById(Guid id)
    {
        return _storageService.GetById(id);
    }

    /// <inheritdoc />
    public StoredFile? RetrieveByName(string fileName)
    {
        return _storageService.GetByFileName(fileName);
    }

    /// <inheritdoc />
    public SignatureVerificationResult VerifyStoredFile(Guid id)
    {
        var file = _storageService.GetById(id);
        if (file is null)
        {
            return SignatureVerificationResult.Invalid($"Stored file with ID '{id}' was not found in memory.");
        }

        return _signatureService.Verify(file.Data, file.Signature, file.PublicKey);
    }

    /// <inheritdoc />
    public async Task<bool> ExportFileAsync(Guid id, string destinationPath, CancellationToken cancellationToken = default)
    {
        var file = _storageService.GetById(id);
        if (file is null)
        {
            return false;
        }

        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(destinationPath, file.Data, cancellationToken);
        return true;
    }
}
