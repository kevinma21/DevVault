using System.Security.Cryptography;
using DevVault.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DevVault.Infrastructure.Security;

public class CryptographyService : ICryptographyService
{
    private readonly byte[] _key;

    public CryptographyService(IConfiguration configuration)
    {
        var base64Key = configuration["Encryption:MasterKey"];
        if (string.IsNullOrEmpty(base64Key))
        {
            throw new ArgumentNullException("Encryption:MasterKey is missing from appsettings.");
        }
        
        _key = Convert.FromBase64String(base64Key);
        
        if (_key.Length != 32)
        {
            throw new ArgumentException("MasterKey must be exactly 32 bytes (256 bits) for AES-256.");
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV(); // Generate a random IV for every encryption

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        
        // Write the IV to the beginning of the stream so we know how to decrypt it later
        ms.Write(aes.IV, 0, aes.IV.Length);
        
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;

        // Extract the IV from the first 16 bytes
        var iv = new byte[aes.BlockSize / 8];
        var cipherBytes = new byte[fullCipher.Length - iv.Length];

        Array.Copy(fullCipher, iv, iv.Length);
        Array.Copy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipherBytes);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}