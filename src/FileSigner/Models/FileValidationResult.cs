namespace FileSigner.Models;

/// <summary>
/// Represents the result of validating a file's name, extension, or size.
/// </summary>
public sealed record FileValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the file satisfies all validation constraints.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the error message explaining why validation failed, or null if valid.
    /// </summary>
    public string? ErrorMessage { get; init; }

    private FileValidationResult(bool isValid, string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static FileValidationResult Success() => new(true, null);

    /// <summary>
    /// Creates a failed validation result with the specified error message.
    /// </summary>
    /// <param name="message">The reason validation failed.</param>
    public static FileValidationResult Failure(string message) => new(false, message);
}
