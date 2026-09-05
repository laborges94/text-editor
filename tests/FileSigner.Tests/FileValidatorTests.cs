using FileSigner.Models;
using FileSigner.Services;
using Xunit;

namespace FileSigner.Tests;

public sealed class FileValidatorTests
{
    private readonly FileValidator _validator = new();

    [Theory]
    [InlineData("document.txt")]
    [InlineData("notes.MD")]
    [InlineData("data.json")]
    [InlineData("schema.xml")]
    [InlineData("records.csv")]
    [InlineData("report.pdf")]
    [InlineData("archive.bin")]
    public void Validate_WithAllowedExtensionAndValidSize_ShouldSucceed(string fileName)
    {
        // Act
        var result = _validator.Validate(fileName, sizeInBytes: 1024);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Theory]
    [InlineData("executable.exe")]
    [InlineData("script.bat")]
    [InlineData("script.ps1")]
    [InlineData("library.dll")]
    [InlineData("archive.zip")]
    public void Validate_WithProhibitedExtension_ShouldFail(string fileName)
    {
        // Act
        var result = _validator.Validate(fileName, sizeInBytes: 1024);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("is not permitted", result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyFileName_ShouldFail(string? fileName)
    {
        // Act
        var result = _validator.Validate(fileName, sizeInBytes: 1024);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("cannot be empty", result.ErrorMessage);
    }

    [Theory]
    [InlineData("noextension")]
    [InlineData("trailingdot.")]
    public void Validate_WithMissingExtension_ShouldFail(string fileName)
    {
        // Act
        var result = _validator.Validate(fileName, sizeInBytes: 1024);

        // Assert
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithZeroOrNegativeSize_ShouldFail(long sizeInBytes)
    {
        // Act
        var result = _validator.Validate("file.txt", sizeInBytes);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("cannot be empty", result.ErrorMessage);
    }

    [Fact]
    public void Validate_WhenSizeExceedsMaximum_ShouldFail()
    {
        // Arrange
        var customOptions = new FileValidationOptions { MaxSizeBytes = 1024 * 1024 }; // 1 MB limit
        var customValidator = new FileValidator(customOptions);

        // Act
        var result = customValidator.Validate("file.txt", sizeInBytes: 1024 * 1024 + 1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("exceeds maximum permitted limit", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFilePath_WithNonExistentFile_ShouldFail()
    {
        // Act
        var result = _validator.ValidateFilePath("C:\\non_existent_file_12345.txt");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("File not found", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFilePath_WithRealTempFile_ShouldSucceed()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.txt");
        File.WriteAllText(tempPath, "Hello, unit test!");

        try
        {
            // Act
            var result = _validator.ValidateFilePath(tempPath);

            // Assert
            Assert.True(result.IsValid);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
