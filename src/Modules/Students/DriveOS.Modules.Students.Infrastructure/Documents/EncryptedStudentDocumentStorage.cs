using System.Buffers.Binary;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using DriveOS.Modules.Students.Application.Documents;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.Extensions.Configuration;

namespace DriveOS.Modules.Students.Infrastructure.Documents;

internal sealed class EncryptedStudentDocumentStorage : IStudentDocumentStorage
{
    private readonly string root;
    private readonly byte[] key;

    public EncryptedStudentDocumentStorage(IConfiguration configuration)
    {
        root = Path.GetFullPath(
            configuration["StudentDocuments:RootPath"]
                ?? Path.Combine(AppContext.BaseDirectory, "private-data", "student-documents")
        );
        string raw =
            configuration["StudentDocuments:EncryptionKeyBase64"]
            ?? throw new InvalidOperationException(
                "StudentDocuments:EncryptionKeyBase64 is required."
            );
        key = Convert.FromBase64String(raw);
        if (key.Length != 32)
            throw new InvalidOperationException(
                "Student document encryption key must contain 32 bytes."
            );
        Directory.CreateDirectory(root);
    }

    public async Task<string> StoreAsync(
        OrganizationId org,
        Guid documentId,
        Guid versionId,
        Stream content,
        CancellationToken ct
    )
    {
        using var source = new MemoryStream();
        await content.CopyToAsync(source, ct);
        byte[] plain = source.ToArray();
        byte[] nonce = RandomNumberGenerator.GetBytes(12),
            tag = new byte[16],
            cipher = new byte[plain.Length];
        using (var aes = new AesGcm(key, 16))
            aes.Encrypt(nonce, plain, cipher, tag);
        string reference = $"{org.Value:N}/{documentId:N}/{versionId:N}.bin";
        string path = Resolve(reference);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var file = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            true
        );
        await file.WriteAsync(nonce, ct);
        await file.WriteAsync(tag, ct);
        await file.WriteAsync(cipher, ct);
        return reference;
    }

    public async Task<Stream?> OpenReadAsync(string reference, CancellationToken ct)
    {
        string path = Resolve(reference);
        if (!File.Exists(path))
            return null;
        byte[] data = await File.ReadAllBytesAsync(path, ct);
        if (data.Length < 28)
            return null;
        byte[] nonce = data[..12],
            tag = data[12..28],
            cipher = data[28..],
            plain = new byte[cipher.Length];
        using (var aes = new AesGcm(key, 16))
            aes.Decrypt(nonce, cipher, tag, plain);
        return new MemoryStream(plain, false);
    }

    private string Resolve(string reference)
    {
        string path = Path.GetFullPath(
            Path.Combine(root, reference.Replace('/', Path.DirectorySeparatorChar))
        );
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid private storage reference.");
        return path;
    }
}

internal sealed class ClamAvStudentDocumentSecurityScanner(IConfiguration configuration)
    : IStudentDocumentSecurityScanner
{
    private readonly string host = configuration["StudentDocuments:ClamAv:Host"] ?? "127.0.0.1";
    private readonly int port = int.TryParse(
        configuration["StudentDocuments:ClamAv:Port"],
        out int p
    )
        ? p
        : 3310;

    public async Task<bool> IsSafeAsync(
        string fileName,
        string contentType,
        ReadOnlyMemory<byte> content,
        CancellationToken ct
    )
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, port, ct);
            await using NetworkStream stream = client.GetStream();
            await stream.WriteAsync(Encoding.ASCII.GetBytes("zINSTREAM\0"), ct);
            const int chunkSize = 8192;
            for (int offset = 0; offset < content.Length; offset += chunkSize)
            {
                int count = Math.Min(chunkSize, content.Length - offset);
                byte[] length = new byte[4];
                BinaryPrimitives.WriteInt32BigEndian(length, count);
                await stream.WriteAsync(length, ct);
                await stream.WriteAsync(content.Slice(offset, count), ct);
            }
            await stream.WriteAsync(new byte[4], ct);
            using var response = new MemoryStream();
            byte[] buffer = new byte[512];
            int read;
            while ((read = await stream.ReadAsync(buffer, ct)) > 0)
            {
                response.Write(buffer, 0, read);
                if (buffer.AsSpan(0, read).Contains((byte)0))
                    break;
            }
            string result = Encoding.UTF8.GetString(response.ToArray());
            return result.Contains("OK", StringComparison.OrdinalIgnoreCase)
                && !result.Contains("FOUND", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
