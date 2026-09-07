# FileSigner Wiki

Welcome to the **FileSigner** repository documentation! This wiki provides a friendly and comprehensive overview of FileSigner, its capabilities, architecture, and how you can use it to secure, sign, and manage files in memory.

---

## 🌟 What is FileSigner?

**FileSigner** is a modern, lightweight, and high-performance **.NET 10** console application designed to guarantee the integrity and authenticity of your files.

In short, it allows you to:
1. **Ingest files** from your local disk or directly as binary byte streams.
2. **Validate files** against strict extension whitelists and configurable size limits.
3. **Cryptographically sign** files using an asymmetric **ECDSA (NIST P-256)** key pair combined with **SHA-256** digests.
4. **Store signed files in memory** using a thread-safe, concurrency-friendly store.
5. **Verify digital signatures** at any time to instantly detect unauthorized tampering or bit rot.
6. **Export or preview** the verified files back to disk or directly in your terminal.

---

## 🚀 Quick Start Guide

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed on your machine.
- Works on **Windows**, **macOS**, and **Linux**.

### Cloning and Building

```bash
# Clone the repository
git clone https://github.com/laborges94/text-editor.git
cd text-editor

# Build the solution
dotnet build

# Run automated tests
dotnet test
```

---

## 🎮 How to Use FileSigner

FileSigner provides two intuitive modes of operation:

### 1. Direct Command-Line Mode (CLI)

Ideal for automated workflows, build pipelines, or quick one-off checks. Pass the path to your file as an argument:

```bash
dotnet run --project src/FileSigner -- "C:\path\to\document.pdf"
```

**Output:**
- Inspects and validates the file.
- Generates its SHA-256 hash and ECDSA digital signature.
- Prints full file metadata, hashes, base64 signature, and public key.
- Immediately runs a cryptographic verification to confirm signature validity.
- Exits with exit code `0` on success, or `1` on error.

---

### 2. Interactive Menu Mode

Run the application without arguments (or with `--interactive`):

```bash
dotnet run --project src/FileSigner
```

You will be greeted by an interactive menu:

```text
============================================================
 FileSigner (.NET 10) - In-Memory Storage & Crypto Signer
============================================================
 Files in Memory: 0 | Total Size: 0 bytes
 1. Ingest, Convert & Cryptographically Sign File from Disk
 2. List All Files in Memory
 3. Retrieve Stored File (Metadata & Content Preview)
 4. Verify Cryptographic Signature of Stored File
 5. Export Stored File to Disk
 6. View Policy Rules & Limits
 7. Clear All Files from Memory
 8. Exit
```

#### Menu Options Explained:
| Option | Name | Description |
|---|---|---|
| **1** | **Ingest File** | Prompts for a file path, validates it against policy rules, signs it, and saves it into memory. |
| **2** | **List Files** | Displays an aligned table of all files currently in memory with IDs, names, sizes, and timestamps. |
| **3** | **Retrieve File** | Look up a stored file by its GUID or name. Displays full cryptographic details and shows a text preview if under 64 KB. |
| **4** | **Verify Signature** | Runs cryptographic verification on an in-memory file to confirm that neither payload nor signature was modified. |
| **5** | **Export File** | Writes the bytes of a stored file back out to any disk destination path. |
| **6** | **View Policy Rules** | Inspect active size limits, allowed extensions, and memory usage. |
| **7** | **Clear Storage** | Wipes all stored files from memory. |
| **8** | **Exit** | Closes the application. |

---

## 🔒 Security & Cryptography Explained

FileSigner uses modern, industry-standard cryptographic primitives:

```
[ Local File / Raw Bytes ]
            │
            ▼
   [ SHA-256 Digest ] ──► Produces a fixed 256-bit (32-byte) hash
            │
            ▼
[ ECDSA Sign (NIST P-256) ] ──► Signs hash with private key
            │
            ▼
[ Stored in Memory ] ──► Payload + SHA-256 + Signature + Public Key (SPKI)
```

- **SHA-256:** Cryptographic hash function. Any single-bit change in the input data results in a completely different hash digest.
- **ECDSA (NIST P-256 / secp256r1):** Elliptic Curve Digital Signature Algorithm. Provides equivalent or superior security to 3072-bit RSA while keeping signature sizes small and execution blazing fast.
- **SPKI Public Key Export:** Public keys are exported and verified using the standard *SubjectPublicKeyInfo* format.
- **Defensive Memory Protection:** Payloads and signatures stored in memory are defensively cloned to ensure that external mutations cannot tamper with stored records.

---

## ⚙️ Default Policy Rules

Out-of-the-box validation rules are configured in `FileValidationOptions`:

- **Maximum Allowed File Size:** `5 MB` (5,242,880 bytes)
- **Minimum File Size:** `> 0 bytes` (empty files are rejected)
- **Allowed File Extensions:**
  - `.txt` (Text documents)
  - `.md` (Markdown files)
  - `.json` (JSON data)
  - `.xml` (XML data)
  - `.csv` (Comma-separated values)
  - `.pdf` (Portable Document Format)
  - `.bin` (Binary payloads)

---

## 🏗️ Architecture Overview

The codebase is organized into clean, single-responsibility services:

- **`Models`**
  - `StoredFile`: Immutable record holding the file payload, metadata, hash, signature, and public key.
  - `CryptographicSignature`: Transfer model holding signature and hash buffers.
  - `FileValidationOptions` & `FileValidationResult`: Strongly-typed validation options and outcomes.
  - `SignatureVerificationResult`: Verification state and descriptive diagnostic message.
- **`Services`**
  - `IFileValidator` / `FileValidator`: Validates extensions, sizes, and file paths.
  - `IDigitalSignatureService` / `EcdsaDigitalSignatureService`: Manages cryptographic key lifecycle, SHA-256 generation, and ECDSA signature creation/verification.
  - `IFileStorageService` / `InMemoryFileStorageService`: Thread-safe in-memory repository backed by `ConcurrentDictionary<Guid, StoredFile>`.
  - `IFileProcessingService` / `FileProcessingService`: High-level facade that coordinates validation, disk reading, signing, storage, and export.
- **`Presentation`**
  - `Program.cs`: CLI entry point, CLI routing, and interactive console loop.

---

## 🧪 Testing

The solution includes automated unit and integration tests using **xUnit**:

```bash
dotnet test
```

Covered test suites:
- Extension whitelist and boundary size testing.
- Cryptographic signature generation and tamper verification.
- In-memory concurrent storage operations.
- Full end-to-end ingestion and disk export.

---

## 💡 Contributing

We welcome community contributions! Please check out our [Contributing Guidelines](../.github/CONTRIBUTING.md) and ensure that all new code is accompanied by unit tests and adheres to the [.editorconfig](../.editorconfig) formatting standards.
