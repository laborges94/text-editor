using System.Text;
using FileSigner.Services;
using Xunit;

namespace FileSigner.Tests;

public sealed class DigitalSignatureServiceTests
{
    [Fact]
    public void ComputeSha256Hash_ShouldMatchExpectedDigest()
    {
        // Arrange
        using var service = new EcdsaDigitalSignatureService();
        var data = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");
        var expectedHex = "d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592";

        // Act
        var hash = service.ComputeSha256Hash(data);
        var actualHex = Convert.ToHexString(hash).ToLowerInvariant();

        // Assert
        Assert.Equal(expectedHex, actualHex);
    }

    [Fact]
    public void Sign_ShouldReturnPopulatedCryptographicSignature()
    {
        // Arrange
        using var service = new EcdsaDigitalSignatureService();
        var data = Encoding.UTF8.GetBytes("Sample file content to be signed.");

        // Act
        var signature = service.Sign(data);

        // Assert
        Assert.NotNull(signature);
        Assert.Equal(32, signature.Sha256Hash.Length);
        Assert.NotEmpty(signature.Signature);
        Assert.NotEmpty(signature.PublicKey);
        Assert.False(string.IsNullOrWhiteSpace(signature.Sha256Hex));
        Assert.False(string.IsNullOrWhiteSpace(signature.SignatureBase64));
        Assert.False(string.IsNullOrWhiteSpace(signature.PublicKeyBase64));
    }

    [Fact]
    public void Verify_WithOriginalData_ShouldBeValid()
    {
        // Arrange
        using var service = new EcdsaDigitalSignatureService();
        var data = Encoding.UTF8.GetBytes("Authentic and untampered document content.");
        var signature = service.Sign(data);

        // Act
        var result = service.Verify(data, signature.Signature, signature.PublicKey);

        // Assert
        Assert.True(result.IsValid);
        Assert.Contains("valid", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Verify_WithTamperedData_ShouldBeInvalid()
    {
        // Arrange
        using var service = new EcdsaDigitalSignatureService();
        var originalData = Encoding.UTF8.GetBytes("Original payload: Amount = $100.00");
        var tamperedData = Encoding.UTF8.GetBytes("Tampered payload: Amount = $9000.00");
        var signature = service.Sign(originalData);

        // Act
        var result = service.Verify(tamperedData, signature.Signature, signature.PublicKey);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("failed", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Verify_WithCorruptedSignature_ShouldBeInvalid()
    {
        // Arrange
        using var service = new EcdsaDigitalSignatureService();
        var data = Encoding.UTF8.GetBytes("Payload for corrupted signature test.");
        var signature = service.Sign(data);

        // Mutate the signature bytes
        var corruptedSignature = (byte[])signature.Signature.Clone();
        corruptedSignature[0] ^= 0xFF;

        // Act
        var result = service.Verify(data, corruptedSignature, signature.PublicKey);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Verify_WithDifferentKey_ShouldBeInvalid()
    {
        // Arrange
        using var signer1 = new EcdsaDigitalSignatureService();
        using var signer2 = new EcdsaDigitalSignatureService();

        var data = Encoding.UTF8.GetBytes("Cross key verification test.");
        var signature = signer1.Sign(data);

        // Act - verify using signer2's public key
        var result = signer1.Verify(data, signature.Signature, signer2.GetPublicKey());

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Sign_WhenDisposed_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var service = new EcdsaDigitalSignatureService();
        service.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => service.Sign(Encoding.UTF8.GetBytes("test")));
    }
}
