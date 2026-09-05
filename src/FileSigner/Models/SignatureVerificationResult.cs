namespace FileSigner.Models;

/// <summary>
/// Represents the outcome of verifying a file's cryptographic signature.
/// </summary>
public sealed record SignatureVerificationResult
{
    /// <summary>
    /// Gets a value indicating whether the signature is cryptographically valid and matches the payload.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets detailed informational or diagnostic message regarding the verification.
    /// </summary>
    public string Message { get; init; }

    private SignatureVerificationResult(bool isValid, string message)
    {
        IsValid = isValid;
        Message = message;
    }

    /// <summary>
    /// Creates a verified result indicating the signature is authentic and unaltered.
    /// </summary>
    public static SignatureVerificationResult Valid(string message = "Signature is cryptographically valid. File content is authentic and untampered.") =>
        new(true, message);

    /// <summary>
    /// Creates a failed verification result indicating signature mismatch or corruption.
    /// </summary>
    public static SignatureVerificationResult Invalid(string reason) =>
        new(false, reason);
}
