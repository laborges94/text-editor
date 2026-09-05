using System.Text;
using FileSigner.Models;
using FileSigner.Services;

namespace FileSigner;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var options = new FileValidationOptions();
        var validator = new FileValidator(options);
        using var signatureService = new EcdsaDigitalSignatureService();
        var storageService = new InMemoryFileStorageService();
        var processingService = new FileProcessingService(validator, signatureService, storageService);

        // Command line direct execution mode
        if (args.Length > 0 && !args[0].Equals("--interactive", StringComparison.OrdinalIgnoreCase))
        {
            return await RunCommandLineModeAsync(args[0], processingService);
        }

        // Interactive console mode
        return await RunInteractiveMenuAsync(processingService, storageService, options);
    }

    private static async Task<int> RunCommandLineModeAsync(string filePath, IFileProcessingService processingService)
    {
        Console.WriteLine("=================================================");
        Console.WriteLine(" FileSigner (.NET 10) - CLI Ingestion & Signing");
        Console.WriteLine("=================================================");
        Console.WriteLine($"Target file: {filePath}");

        var (success, errorMessage, file) = await processingService.ProcessAndStoreFileAsync(filePath);
        if (!success || file is null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {errorMessage}");
            Console.ResetColor();
            return 1;
        }

        PrintFileDetails(file);

        var verification = processingService.VerifyStoredFile(file.Id);
        PrintVerificationResult(verification);

        return 0;
    }

    private static async Task<int> RunInteractiveMenuAsync(
        IFileProcessingService processingService,
        IFileStorageService storageService,
        FileValidationOptions options)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine(" FileSigner (.NET 10) - In-Memory Storage & Crypto Signer");
            Console.WriteLine("============================================================");
            Console.WriteLine($" Files in Memory: {storageService.Count} | Total Size: {storageService.TotalMemoryBytes:N0} bytes");
            Console.WriteLine(" 1. Ingest, Convert & Cryptographically Sign File from Disk");
            Console.WriteLine(" 2. List All Files in Memory");
            Console.WriteLine(" 3. Retrieve Stored File (Metadata & Content Preview)");
            Console.WriteLine(" 4. Verify Cryptographic Signature of Stored File");
            Console.WriteLine(" 5. Export Stored File to Disk");
            Console.WriteLine(" 6. View Policy Rules & Limits");
            Console.WriteLine(" 7. Clear All Files from Memory");
            Console.WriteLine(" 8. Exit");
            Console.Write("\nSelect an option (1-8): ");

            var input = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (input)
            {
                case "1":
                    await HandleIngestFileAsync(processingService);
                    break;
                case "2":
                    HandleListFiles(storageService);
                    break;
                case "3":
                    HandleRetrieveFile(processingService, storageService);
                    break;
                case "4":
                    HandleVerifySignature(processingService, storageService);
                    break;
                case "5":
                    await HandleExportFileAsync(processingService, storageService);
                    break;
                case "6":
                    HandleViewPolicy(options, storageService);
                    break;
                case "7":
                    storageService.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[INFO] In-memory storage cleared successfully.");
                    Console.ResetColor();
                    break;
                case "8":
                    Console.WriteLine("Exiting FileSigner. Goodbye!");
                    return 0;
                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[WARNING] Invalid option selected. Please choose between 1 and 8.");
                    Console.ResetColor();
                    break;
            }
        }
    }

    private static async Task HandleIngestFileAsync(IFileProcessingService processingService)
    {
        Console.Write("Enter path to file on disk: ");
        var path = Console.ReadLine()?.Trim(' ', '"');

        if (string.IsNullOrWhiteSpace(path))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[WARNING] No file path provided.");
            Console.ResetColor();
            return;
        }

        var (success, errorMessage, file) = await processingService.ProcessAndStoreFileAsync(path);
        if (!success || file is null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[REJECTED] {errorMessage}");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[SUCCESS] File ingested, converted to bytes, signed, and persisted in memory.");
        Console.ResetColor();

        PrintFileDetails(file);

        var verification = processingService.VerifyStoredFile(file.Id);
        PrintVerificationResult(verification);
    }

    private static void HandleListFiles(IFileStorageService storageService)
    {
        var files = storageService.GetAll();
        if (files.Count == 0)
        {
            Console.WriteLine("No files are currently stored in memory.");
            return;
        }

        Console.WriteLine($"{"ID",-38} | {"File Name",-25} | {"Size (bytes)",-12} | {"Ingested At",-20}");
        Console.WriteLine(new string('-', 100));

        foreach (var file in files)
        {
            Console.WriteLine($"{file.Id,-38} | {file.FileName,-25} | {file.SizeInBytes,-12:N0} | {file.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }
    }

    private static void HandleRetrieveFile(IFileProcessingService processingService, IFileStorageService storageService)
    {
        var file = PromptAndFindFile(storageService);
        if (file is null)
        {
            return;
        }

        PrintFileDetails(file);

        // Preview text if applicable
        var textExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".md", ".json", ".xml", ".csv"
        };

        if (textExtensions.Contains(file.Extension) && file.SizeInBytes < 64 * 1024)
        {
            Console.WriteLine("\n--- Content Preview ---");
            var content = Encoding.UTF8.GetString(file.Data);
            Console.WriteLine(content);
            Console.WriteLine("--- End of Preview ---\n");
        }
    }

    private static void HandleVerifySignature(IFileProcessingService processingService, IFileStorageService storageService)
    {
        var file = PromptAndFindFile(storageService);
        if (file is null)
        {
            return;
        }

        var verification = processingService.VerifyStoredFile(file.Id);
        PrintVerificationResult(verification);
    }

    private static async Task HandleExportFileAsync(IFileProcessingService processingService, IFileStorageService storageService)
    {
        var file = PromptAndFindFile(storageService);
        if (file is null)
        {
            return;
        }

        Console.Write("Enter destination file path: ");
        var destPath = Console.ReadLine()?.Trim(' ', '"');
        if (string.IsNullOrWhiteSpace(destPath))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[WARNING] Export canceled: path cannot be empty.");
            Console.ResetColor();
            return;
        }

        var exported = await processingService.ExportFileAsync(file.Id, destPath);
        if (exported)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SUCCESS] Stored file '{file.FileName}' exported to '{destPath}'.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR] Failed to export file.");
            Console.ResetColor();
        }
    }

    private static void HandleViewPolicy(FileValidationOptions options, IFileStorageService storageService)
    {
        Console.WriteLine("Policy & Validation Rules:");
        Console.WriteLine($"  - Max Allowed File Size : {options.MaxSizeBytes / (1024.0 * 1024.0):F2} MB ({options.MaxSizeBytes:N0} bytes)");
        Console.WriteLine($"  - Allowed Extensions    : {string.Join(", ", options.AllowedExtensions)}");
        Console.WriteLine($"  - Cryptographic Method  : ECDSA (NIST P-256) with SHA-256 Digest");
        Console.WriteLine($"  - Current Memory Usage  : {storageService.TotalMemoryBytes:N0} bytes across {storageService.Count} file(s)");
    }

    private static StoredFile? PromptAndFindFile(IFileStorageService storageService)
    {
        if (storageService.Count == 0)
        {
            Console.WriteLine("No files stored in memory.");
            return null;
        }

        Console.Write("Enter File ID (Guid) or File Name: ");
        var query = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[WARNING] Input cannot be empty.");
            Console.ResetColor();
            return null;
        }

        StoredFile? file;
        if (Guid.TryParse(query, out var id))
        {
            file = storageService.GetById(id);
        }
        else
        {
            file = storageService.GetByFileName(query);
        }

        if (file is null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARNING] No file found matching '{query}'.");
            Console.ResetColor();
            return null;
        }

        return file;
    }

    private static void PrintFileDetails(StoredFile file)
    {
        Console.WriteLine();
        Console.WriteLine("--- Stored File Details ---");
        Console.WriteLine($"ID              : {file.Id}");
        Console.WriteLine($"File Name       : {file.FileName}");
        Console.WriteLine($"Extension       : {file.Extension}");
        Console.WriteLine($"Size            : {file.SizeInBytes:N0} bytes");
        Console.WriteLine($"Ingested At     : {file.CreatedAt:yyyy-MM-dd HH:mm:ss 'UTC'}");
        Console.WriteLine($"SHA-256 Digest  : {Convert.ToHexString(file.Sha256Hash).ToLowerInvariant()}");
        Console.WriteLine($"Digital Signature (Base64) : {Convert.ToBase64String(file.Signature)}");
        Console.WriteLine($"Public Key (Base64)        : {Convert.ToBase64String(file.PublicKey)}");
        Console.WriteLine("---------------------------\n");
    }

    private static void PrintVerificationResult(SignatureVerificationResult result)
    {
        if (result.IsValid)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SIGNATURE VALID] {result.Message}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[SIGNATURE INVALID] {result.Message}");
        }
        Console.ResetColor();
    }
}
