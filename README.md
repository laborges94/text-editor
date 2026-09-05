# FileSigner

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![.NET Version](https://img.shields.io/badge/.NET-10.0-blue.svg)

A high-performance .NET 10 console application designed to ingest files, convert them to binary representation, store them in memory, and generate cryptographically valid digital signatures (SHA-256 hash + ECDSA P-256).

## 🚀 Features

- **File Validation**: Strict file extension whitelist (`.txt`, `.md`, `.json`, `.xml`, `.csv`, `.pdf`, `.bin`) and file size limits (default 5 MB).
- **In-Memory Storage**: Thread-safe storage holding raw byte payloads, metadata, hashes, and cryptographic signatures.
- **File Retrieval**: Lookup and export stored files by unique ID or file name.
- **Cryptographic Signing**: Digitally signs file data using SHA-256 and ECDSA (Elliptic Curve Digital Signature Algorithm).
- **Cryptographic Verification**: Verifies signatures against file contents and public keys to detect tampering.

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (version 10.0.100 or later)
- Operating System: Windows, macOS, or Linux

## 🛠️ Getting Started

### Installation

Clone the repository:

```bash
git clone https://github.com/laborges94/text-editor.git
cd text-editor
```

### Build the project

```bash
dotnet build
```

### Run the application

```bash
dotnet run --project src/FileSigner
```

### Run tests

```bash
dotnet test
```

## 📂 Project Structure

```
├── .github/
│   ├── workflows/              # GitHub Actions CI/CD pipelines
│   ├── CONTRIBUTING.md         # Contribution guidelines
│   ├── copilot-instructions.md # Coding styles and prompt configurations
│   └── pull_request_template.md
├── src/
│   └── FileSigner/             # .NET 10 Console Application
│       ├── Models/             # Domain records and options
│       ├── Services/           # Storage, validator, and crypto services
│       └── Program.cs          # Console entry point & interactive CLI
├── tests/
│   └── FileSigner.Tests/       # Unit & Integration tests (xUnit)
├── .editorconfig               # Formatting and style rules
├── .gitattributes              # Line endings configuration
├── .gitignore                  # Git ignore patterns
├── AGENTS.md                   # Instructions for AI coding assistants
├── LICENSE                     # Open source license (MIT)
├── README.md                   # Project documentation
└── FileSigner.slnx             # Solution file
```

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to check the [Contributing Guidelines](.github/CONTRIBUTING.md).

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
