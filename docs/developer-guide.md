# Guia do Desenvolvedor - Arquitetura e Funcionamento do FileSigner

Este documento contém o detalhamento técnico completo da aplicação **FileSigner**, voltado para os desenvolvedores e mantenedores do projeto.

---

## 1. Sumário de Tópicos da Aplicação

1. **Visão Geral e Propósito**
2. **Fluxo de Execução e Modos de Operação**
   - Modo CLI Direto (Linha de Comando)
   - Modo Console Interativo
3. **Módulo de Validação e Políticas de Arquivo (`FileValidator`)**
   - Validação de Extensões Permitidas
   - Validação de Limites de Tamanho
   - Validação de Existência e Caminho
4. **Módulo Criptográfico e Assinaturas Digitais (`EcdsaDigitalSignatureService`)**
   - Digest SHA-256
   - Algoritmo ECDSA (Curva NIST P-256)
   - Exportação e Importação de Chave Pública (SPKI)
   - Verificação de Integridade e Autenticidade
5. **Módulo de Armazenamento em Memória (`InMemoryFileStorageService`)**
   - Estrutura de Armazenamento Concorrente (`ConcurrentDictionary`)
   - Imutabilidade e Clonagem Defensiva de Bytes
   - Mecanismos de Consulta (ID e Nome)
   - Métricas de Uso de Memória
6. **Módulo de Orquestração (`FileProcessingService`)**
   - Fluxo de Ingestão de Arquivo em Disco
   - Fluxo de Ingestão Direta de Bytes
   - Verificação e Exportação de Arquivos
7. **Modelos de Dados (`Models`)**
   - `StoredFile`
   - `CryptographicSignature`
   - `FileValidationOptions` e `FileValidationResult`
   - `SignatureVerificationResult`
8. **Segurança e Thread Safety**
9. **Estratégia de Testes Automatizados**
10. **Guia de Extensibilidade e Boas Práticas**

---

## 2. Detalhamento e Explicação de Cada Tópico

### 2.1. Visão Geral e Propósito
O **FileSigner** é uma aplicação de console construída em **.NET 10** focada em segurança, integridade de dados e alta performance.
Seu objetivo principal é:
- Receber arquivos locais ou payloads binários em memória;
- Validar se o arquivo respeita as políticas do sistema (tamanho e formato);
- Gerar uma representação binária fiel do conteúdo;
- Produzir uma assinatura digital assimétrica utilizando criptografia de curva elíptica (**ECDSA P-256**) combinada com resumo criptográfico (**SHA-256**);
- Armazenar o arquivo assinado em uma estrutura segura em memória;
- Permitir a auditoria/verificação da assinatura a qualquer momento, garantindo que o arquivo não sofreu adulteração;
- Permitir a exportação do arquivo armazenado de volta ao disco.

---

### 2.2. Fluxo de Execução e Modos de Operação
A classe [`Program`](../src/FileSigner/Program.cs) é o ponto de entrada (`Main`) e suporta dois modos:

#### A. Modo CLI Direto (Headless / Scripting)
- **Como é acionado:** Passando o caminho de um arquivo como argumento via linha de comando, por exemplo:
  ```bash
  dotnet run --project src/FileSigner -- "C:\documentos\relatorio.pdf"
  ```
- **Comportamento:**
  1. Configura a saída do console para UTF-8 (`Console.OutputEncoding = Encoding.UTF8`).
  2. Inicializa os serviços necessários via injeção direta de dependências.
  3. Executa `FileProcessingService.ProcessAndStoreFileAsync(filePath)`.
  4. Imprime no console os metadados do arquivo (ID, Hash SHA-256, Assinatura em Base64, Chave Pública em Base64).
  5. Imediatamente roda `VerifyStoredFile(file.Id)` para confirmar a validade da assinatura recém-criada.
  6. Retorna código de saída `0` em caso de sucesso ou `1` em caso de falha.

#### B. Modo Console Interativo
- **Como é acionado:** Executando a aplicação sem argumentos ou com `--interactive`.
- **Menu de opções:**
  1. **Ingest, Convert & Cryptographically Sign File from Disk:** Solicita o caminho completo do arquivo, valida, assina, armazena e exibe o status de verificação.
  2. **List All Files in Memory:** Lista em formato tabular todos os arquivos presentes na memória, exibindo ID (GUID), nome do arquivo, tamanho em bytes e data de ingestão (UTC).
  3. **Retrieve Stored File:** Permite buscar um arquivo pelo seu GUID ou pelo nome. Exibe detalhes completos e, caso seja um arquivo de texto (`.txt`, `.md`, `.json`, `.xml`, `.csv`) menor que 64 KB, renderiza uma prévia do conteúdo no terminal.
  4. **Verify Cryptographic Signature of Stored File:** Executa a verificação matemática da assinatura contra os bytes e a chave pública armazenada.
  5. **Export Stored File to Disk:** Salva os bytes mantidos em memória de volta em um arquivo de destino especificado pelo usuário (criando diretórios caso não existam).
  6. **View Policy Rules & Limits:** Exibe as regras ativas de validação (tamanho máximo permitido, extensões aceitas, algoritmo criptográfico e consumo atual de memória).
  7. **Clear All Files from Memory:** Esvazia o repositório em memória.
  8. **Exit:** Finaliza a aplicação.

---

### 2.3. Módulo de Validação e Políticas de Arquivo (`FileValidator`)
Localizado em [`src/FileSigner/Services/FileValidator.cs`](../src/FileSigner/Services/FileValidator.cs), este componente assegura que nenhum arquivo fora do padrão entre no ciclo de processamento.

- **Opções Configuráveis (`FileValidationOptions`):**
  - `MaxSizeBytes`: Limite máximo em bytes (por padrão 5.242.880 bytes, ou seja, 5 MB).
  - `AllowedExtensions`: `HashSet<string>` insensível a maiúsculas/minúsculas (`StringComparer.OrdinalIgnoreCase`) contendo:
    - `.txt`, `.md`, `.json`, `.xml`, `.csv`, `.pdf`, `.bin`.
- **Regras de Validação:**
  1. **Nulidade ou Espaço em Branco:** Nome de arquivo ou caminho vazio é rejeitado imediatamente.
  2. **Verificação de Extensão:** Arquivos sem extensão ou com extensão fora da lista de permissões são barrados.
  3. **Verificação de Tamanho Mínimo:** Arquivos com tamanho `<= 0` (vazios) são rejeitados.
  4. **Verificação de Tamanho Máximo:** Arquivos com tamanho superior ao `MaxSizeBytes` são rejeitados com mensagem amigável contendo o valor em MB e bytes.
  5. **Verificação de Existência:** No método `ValidateFilePath`, o validador confere se o arquivo físico existe no sistema de arquivos antes da leitura.

---

### 2.4. Módulo Criptográfico e Assinaturas Digitais (`EcdsaDigitalSignatureService`)
Localizado em [`src/FileSigner/Services/EcdsaDigitalSignatureService.cs`](../src/FileSigner/Services/EcdsaDigitalSignatureService.cs).

- **Digest SHA-256:**
  - O conteúdo binário do arquivo é processado através de `SHA256.HashData(data)`, gerando um resumo criptográfico de 32 bytes (256 bits).
- **Algoritmo ECDSA (Curva NIST P-256):**
  - Utiliza `ECDsa.Create(ECCurve.NamedCurves.nistP256)`. A criptografia assimétrica de curva elíptica oferece alto nível de segurança com tamanhos de chave e assinaturas compactos em comparação com RSA tradicional.
- **Exportação da Chave Pública (SPKI):**
  - A chave pública é exportada no padrão *SubjectPublicKeyInfo* (`ExportSubjectPublicKeyInfo()`), formato padrão interoperável para distribuição de chaves públicas.
- **Assinatura (`Sign`):**
  - Executa a assinatura do hash com `_ecdsa.SignHash(hash)`.
  - É protegido por `lock (_lock)` para garantir segurança contra condições de corrida durante a assinatura.
- **Verificação (`Verify`):**
  - Cria uma instância efêmera de `ECDsa`, importa a chave pública fornecida via `ImportSubjectPublicKeyInfo`, recalcula o hash SHA-256 dos dados e executa `verifier.VerifyHash(hash, signature)`.
  - Se os dados ou a assinatura forem alterados em apenas 1 bit, a verificação falhará imediatamente.
- **Descarte de Recursos (`IDisposable`):**
  - A classe descarta apropriadamente os identificadores de chave do sistema operacional nativo chamando `_ecdsa.Dispose()`.

---

### 2.5. Módulo de Armazenamento em Memória (`InMemoryFileStorageService`)
Localizado em [`src/FileSigner/Services/InMemoryFileStorageService.cs`](../src/FileSigner/Services/InMemoryFileStorageService.cs).

- **Estrutura Thread-Safe:**
  - Utiliza `ConcurrentDictionary<Guid, StoredFile>` para armazenamento. Operações concorrentes de adição, leitura e remoção são seguras para múltiplas threads.
- **Clonagem Defensiva de Buffers:**
  - Ao armazenar um arquivo, os arrays de bytes (`Data`, `Sha256Hash`, `Signature`, `PublicKey`) passam por `.Clone()`. Isso impede que código externo altere o conteúdo dos bytes após a inserção.
- **Consultas Rápidas:**
  - Busca por chave primária (`Guid`): Complexidade $O(1)$ através de `TryGetValue`.
  - Busca por nome de arquivo (`FileName`): Comparação insensível a maiúsculas/minúsculas usando LINQ `FirstOrDefault`.
- **Métricas:**
  - `Count`: Quantidade de arquivos em memória.
  - `TotalMemoryBytes`: Soma do tamanho de todos os payloads mantidos em memória.

---

### 2.6. Módulo de Orquestração (`FileProcessingService`)
Localizado em [`src/FileSigner/Services/FileProcessingService.cs`](../src/FileSigner/Services/FileProcessingService.cs).
Este serviço atua como a fachada da camada de negócio e coordena os componentes:

1. **`ProcessAndStoreFileAsync(filePath)`**:
   - Valida caminho e regras com `IFileValidator`.
   - Lê os bytes do disco de forma assíncrona (`File.ReadAllBytesAsync`).
   - Assina os dados via `IDigitalSignatureService`.
   - Armazena no repositório `IFileStorageService`.
   - Retorna uma tupla contendo `(Success, ErrorMessage, StoredFile)`.
2. **`ProcessAndStoreBytes(fileName, data)`**:
   - Idem ao anterior, porém recebe o payload de bytes já em memória, permitindo integração direta com APIs, streams ou testes.
3. **`VerifyStoredFile(id)`**:
   - Recupera o registro do arquivo e valida os dados contra a assinatura e a chave pública armazenadas.
4. **`ExportFileAsync(id, destinationPath)`**:
   - Recupera os bytes do arquivo em memória, garante a criação do diretório de destino e grava os bytes assincronamente no disco (`File.WriteAllBytesAsync`).

---

### 2.7. Modelos de Dados (`Models`)

- **`StoredFile`**:
  Representa o arquivo arquivado em memória.
  ```csharp
  public sealed record StoredFile(
      Guid Id,
      string FileName,
      string Extension,
      long SizeInBytes,
      byte[] Data,
      byte[] Sha256Hash,
      byte[] Signature,
      byte[] PublicKey,
      DateTimeOffset CreatedAt);
  ```
- **`CryptographicSignature`**:
  Objeto de transporte com os artefatos de uma assinatura recém-criada.
  ```csharp
  public sealed record CryptographicSignature(
      byte[] Sha256Hash,
      byte[] Signature,
      byte[] PublicKey);
  ```
- **`FileValidationOptions`**:
  Contém as opções de validação (`MaxSizeBytes` e `AllowedExtensions`).
- **`FileValidationResult`**:
  Indica se a validação passou (`IsValid`) e traz eventual `ErrorMessage`.
- **`SignatureVerificationResult`**:
  Informa se a assinatura é matematicamente válida (`IsValid`) acompanhada de uma mensagem detalhada (`Message`).

---

### 2.8. Segurança e Thread Safety
1. **Thread Safety em Operações Criptográficas:** O objeto `ECDsa` nativo em .NET não é necessariamente seguro para múltiplas chamadas simultâneas de assinatura; por isso, o serviço utiliza um bloco de exclusão mútua (`lock`) no método `Sign`.
2. **Proteção de Memória:** Como os arrays binários são clonados na entrada, evita-se vazamento de mutações acidentais de buffers compartilhados.
3. **Detecção de Adulteração:** A combinação de SHA-256 e ECDSA garante não-repúdio e integridade: mesmo que o conteúdo sofra a alteração de 1 bit, o hash SHA-256 mudará drasticamente (efeito avalanche) e a assinatura falhará.

---

### 2.9. Estratégia de Testes Automatizados
O projeto possui uma suíte completa de testes unitários no projeto `tests/FileSigner.Tests` cobrindo:
- **`FileValidatorTests`**: Validação de arquivos vazios, extensões não autorizadas, arquivos que ultrapassam o tamanho limite e arquivos válidos.
- **`DigitalSignatureServiceTests`**: Geração de chaves, verificação de assinaturas válidas, falha de verificação ao corromper o payload ou a assinatura, e testes com instâncias injetadas de ECDSA.
- **`InMemoryFileStorageServiceTests`**: Persistência concorrente, consulta por ID, consulta por nome, exclusão e limpeza da memória.
- **`FileProcessingServiceTests`**: Fluxo ponta a ponta de processamento em memória e exportação.

---

### 2.10. Guia de Extensibilidade e Boas Práticas
Para desenvolvedores que desejam estender a aplicação:
- **Novas Políticas de Armazenamento:** Implementar `IFileStorageService` para persistir dados em Redis, SQLite, Azure Blob Storage ou AWS S3 sem impactar a interface do console.
- **Novos Algoritmos de Assinatura:** Implementar `IDigitalSignatureService` para suportar RSA (PKCS#1 ou PSS) ou Ed25519.
- **Injeção de Dependências com Host Genérico:** Atualmente o `Program.cs` monta o grafo de dependências manualmente; é possível migrar para `Microsoft.Extensions.Hosting` para usar `IServiceCollection` e configuração via `appsettings.json`.
