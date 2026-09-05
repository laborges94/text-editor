using System.Collections.Concurrent;
using FileSigner.Models;

namespace FileSigner.Services;

/// <summary>
/// Thread-safe in-memory repository for storing and retrieving files and their cryptographic signatures.
/// </summary>
public sealed class InMemoryFileStorageService : IFileStorageService
{
    private readonly ConcurrentDictionary<Guid, StoredFile> _files = new();

    /// <inheritdoc />
    public int Count => _files.Count;

    /// <inheritdoc />
    public long TotalMemoryBytes => _files.Values.Sum(f => f.SizeInBytes);

    /// <inheritdoc />
    public StoredFile Store(string fileName, byte[] data, CryptographicSignature signature)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(signature);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var storedFile = new StoredFile(
            Id: Guid.NewGuid(),
            FileName: Path.GetFileName(fileName),
            Extension: extension,
            SizeInBytes: data.LongLength,
            Data: (byte[])data.Clone(),
            Sha256Hash: (byte[])signature.Sha256Hash.Clone(),
            Signature: (byte[])signature.Signature.Clone(),
            PublicKey: (byte[])signature.PublicKey.Clone(),
            CreatedAt: DateTimeOffset.UtcNow);

        _files[storedFile.Id] = storedFile;
        return storedFile;
    }

    /// <inheritdoc />
    public StoredFile? GetById(Guid id)
    {
        return _files.TryGetValue(id, out var file) ? file : null;
    }

    /// <inheritdoc />
    public StoredFile? GetByFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var normalizedTarget = Path.GetFileName(fileName);
        return _files.Values.FirstOrDefault(f =>
            string.Equals(f.FileName, normalizedTarget, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public IReadOnlyCollection<StoredFile> GetAll()
    {
        return _files.Values.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public bool Delete(Guid id)
    {
        return _files.TryRemove(id, out _);
    }

    /// <inheritdoc />
    public void Clear()
    {
        _files.Clear();
    }
}
