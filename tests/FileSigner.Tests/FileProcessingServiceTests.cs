using System.Text;
using FileSigner.Models;
using FileSigner.Services;
using Xunit;

namespace FileSigner.Tests;

public sealed class FileProcessingServiceTests : IDisposable
{
    private readonly EcdsaDigitalSignatureService _signatureService = new();
    private readonly FileValidator _validator = new(new FileValidationOptions { MaxSizeBytes = 10 * 1024 });
    private readonly InMemoryFileStorageService _storageService = new();
    private readonly FileProcessingService _service;

    public FileProcessingServiceTests()
    {
        _service = new FileProcessingService(_validator, _signatureService, _storageService);
    }

    [Fact]
    public void ProcessAndStoreBytes_WithValidData_ShouldSucceedAndSign()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("Valid in-memory payload.");

        // Act
        var (success, errorMessage, file) = _service.ProcessAndStoreBytes("payload.json", content);

        // Assert
        Assert.True(success);
        Assert.Null(errorMessage);
        Assert.NotNull(file);
        Assert.Equal("payload.json", file.FileName);
        Assert.Equal(".json", file.Extension);
        Assert.Equal(content.Length, file.SizeInBytes);
        Assert.NotEmpty(file.Sha256Hash);
        Assert.NotEmpty(file.Signature);

        // Verify stored in repository
        var retrieved = _service.RetrieveById(file.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(file.Id, retrieved.Id);
    }

    [Fact]
    public void ProcessAndStoreBytes_WithInvalidExtension_ShouldFail()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("binary payload");

        // Act
        var (success, errorMessage, file) = _service.ProcessAndStoreBytes("malicious.exe", content);

        // Assert
        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Contains("not permitted", errorMessage);
        Assert.Null(file);
    }

    [Fact]
    public void ProcessAndStoreBytes_WithOversizedData_ShouldFail()
    {
        // Arrange - validator limit is 10 KB in this test fixture
        var oversizedContent = new byte[11 * 1024];

        // Act
        var (success, errorMessage, file) = _service.ProcessAndStoreBytes("large.txt", oversizedContent);

        // Assert
        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Contains("exceeds maximum permitted limit", errorMessage);
        Assert.Null(file);
    }

    [Fact]
    public async Task ProcessAndStoreFileAsync_WithPhysicalFile_ShouldSucceed()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.md");
        await File.WriteAllTextAsync(tempFile, "# Hello Markdown");

        try
        {
            // Act
            var (success, errorMessage, file) = await _service.ProcessAndStoreFileAsync(tempFile);

            // Assert
            Assert.True(success);
            Assert.Null(errorMessage);
            Assert.NotNull(file);
            Assert.Equal(Path.GetFileName(tempFile), file.FileName);

            // Verify signature
            var verification = _service.VerifyStoredFile(file.Id);
            Assert.True(verification.IsValid);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ExportFileAsync_ShouldWriteIdenticalBytesToDisk()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("Content to be exported and verified.");
        var (_, _, file) = _service.ProcessAndStoreBytes("export_test.txt", content);
        Assert.NotNull(file);

        var tempExportPath = Path.Combine(Path.GetTempPath(), $"exported_{Guid.NewGuid():N}.txt");

        try
        {
            // Act
            var exported = await _service.ExportFileAsync(file.Id, tempExportPath);

            // Assert
            Assert.True(exported);
            Assert.True(File.Exists(tempExportPath));

            var exportedBytes = await File.ReadAllBytesAsync(tempExportPath);
            Assert.Equal(content, exportedBytes);
        }
        finally
        {
            if (File.Exists(tempExportPath))
            {
                File.Delete(tempExportPath);
            }
        }
    }

    public void Dispose()
    {
        _signatureService.Dispose();
    }
}
