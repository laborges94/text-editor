namespace FileSigner.Models;

/// <summary>
/// Configuration options governing file validation rules.
/// </summary>
public sealed class FileValidationOptions
{
    /// <summary>
    /// Default maximum file size: 5 Megabytes (5 * 1024 * 1024 bytes).
    /// </summary>
    public const long DefaultMaxSizeBytes = 5 * 1024 * 1024;

    /// <summary>
    /// Default standard set of permitted file extensions.
    /// </summary>
    public static readonly string[] DefaultAllowedExtensions =
    [
        ".txt",
        ".md",
        ".json",
        ".xml",
        ".csv",
        ".pdf",
        ".bin"
    ];

    /// <summary>
    /// Gets or sets the maximum allowed file size in bytes.
    /// </summary>
    public long MaxSizeBytes { get; set; } = DefaultMaxSizeBytes;

    /// <summary>
    /// Gets or sets the set of allowed file extensions (case-insensitive, with leading dot).
    /// </summary>
    public HashSet<string> AllowedExtensions { get; set; } =
        new(DefaultAllowedExtensions, StringComparer.OrdinalIgnoreCase);
}
