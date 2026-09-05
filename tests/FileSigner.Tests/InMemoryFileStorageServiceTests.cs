using System.Text;
using FileSigner.Models;
using FileSigner.Services;
using Xunit;

namespace FileSigner.Tests;

public sealed class InMemoryFileStorageServiceTests
{
    private readonly InMemoryFileStorageService _storage = new();
    private readonly CryptographicSignature _dummySignature = new(
        Sha256Hash: new byte[32],
        Signature: new byte[64],
        PublicKey: new byte[91]);

    [Fact]
    public void Store_ShouldPersistFileAndReturnStoredFile()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("Sample content");

        // Act
        var stored = _storage.Store("document.txt", content, _dummySignature);

        // Assert
        Assert.NotNull(stored);
        Assert.NotEqual(Guid.Empty, stored.Id);
        Assert.Equal("document.txt", stored.FileName);
        Assert.Equal(".txt", stored.Extension);
        Assert.Equal(content.Length, stored.SizeInBytes);
        Assert.Equal(1, _storage.Count);
        Assert.Equal(content.Length, _storage.TotalMemoryBytes);
    }

    [Fact]
    public void GetById_WhenFileExists_ShouldReturnFile()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("Another file content");
        var stored = _storage.Store("notes.md", content, _dummySignature);

        // Act
        var retrieved = _storage.GetById(stored.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(stored.Id, retrieved.Id);
        Assert.Equal("notes.md", retrieved.FileName);
    }

    [Fact]
    public void GetById_WhenFileDoesNotExist_ShouldReturnNull()
    {
        // Act
        var retrieved = _storage.GetById(Guid.NewGuid());

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public void GetByFileName_ShouldBeCaseInsensitive()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("Case sensitivity test");
        _storage.Store("Report.JSON", content, _dummySignature);

        // Act
        var retrieved = _storage.GetByFileName("report.json");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Report.JSON", retrieved.FileName);
    }

    [Fact]
    public void Store_ShouldDefensivelyCopyData()
    {
        // Arrange
        var originalBytes = new byte[] { 1, 2, 3, 4, 5 };
        var stored = _storage.Store("data.bin", originalBytes, _dummySignature);

        // Act - mutate the original buffer
        originalBytes[0] = 99;

        // Assert - stored file data must remain unchanged
        Assert.Equal(1, stored.Data[0]);
    }

    [Fact]
    public void Delete_WhenFileExists_ShouldRemoveFile()
    {
        // Arrange
        var stored = _storage.Store("temp.txt", [1, 2, 3], _dummySignature);
        Assert.Equal(1, _storage.Count);

        // Act
        var deleted = _storage.Delete(stored.Id);

        // Assert
        Assert.True(deleted);
        Assert.Equal(0, _storage.Count);
        Assert.Null(_storage.GetById(stored.Id));
    }

    [Fact]
    public void Clear_ShouldRemoveAllFiles()
    {
        // Arrange
        _storage.Store("file1.txt", [1, 2], _dummySignature);
        _storage.Store("file2.txt", [3, 4], _dummySignature);
        Assert.Equal(2, _storage.Count);

        // Act
        _storage.Clear();

        // Assert
        Assert.Equal(0, _storage.Count);
        Assert.Equal(0, _storage.TotalMemoryBytes);
        Assert.Empty(_storage.GetAll());
    }
}
