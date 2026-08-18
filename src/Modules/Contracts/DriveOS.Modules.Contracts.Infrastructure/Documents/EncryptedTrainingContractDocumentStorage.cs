using System.Security.Cryptography;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Generate;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.Extensions.Configuration;

namespace DriveOS.Modules.Contracts.Infrastructure.Documents;

internal sealed class EncryptedTrainingContractDocumentStorage : ITrainingContractDocumentStorage
{
    private readonly string root;
    private readonly byte[] key;

    public EncryptedTrainingContractDocumentStorage(IConfiguration configuration)
    {
        root = Path.GetFullPath(configuration["Contracts:Documents:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "private-data", "contracts"));
        string raw = configuration["Contracts:Documents:EncryptionKeyBase64"]
            ?? configuration["StudentDocuments:EncryptionKeyBase64"]
            ?? throw new InvalidOperationException("Contracts:Documents:EncryptionKeyBase64 is required.");
        key = Convert.FromBase64String(raw);
        if (key.Length != 32) throw new InvalidOperationException("Contract document encryption key must contain 32 bytes.");
        Directory.CreateDirectory(root);
    }

    public async Task<string> StoreAsync(OrganizationId organizationId, TrainingContractId contractId, int versionNumber, string fileName, ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default)
    {
        byte[] nonce = RandomNumberGenerator.GetBytes(12), tag = new byte[16], cipher = new byte[content.Length];
        using (var aes = new AesGcm(key, 16)) aes.Encrypt(nonce, content.Span, cipher, tag);
        string safeName = string.Concat(fileName.Select(c => char.IsLetterOrDigit(c) || c is '.' or '-' or '_' ? c : '_'));
        string reference = $"{organizationId.Value:N}/{contractId.Value:N}/v{versionNumber}/{Guid.NewGuid():N}-{safeName}.bin";
        string path = Resolve(reference);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        await stream.WriteAsync(nonce, cancellationToken);
        await stream.WriteAsync(tag, cancellationToken);
        await stream.WriteAsync(cipher, cancellationToken);
        return reference;
    }

    private string Resolve(string reference)
    {
        string path = Path.GetFullPath(Path.Combine(root, reference.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal)) throw new InvalidOperationException("Invalid private contract storage reference.");
        return path;
    }
}
