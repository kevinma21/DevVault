namespace DevVault.Application.Interfaces;

public interface ICryptographyService
{
    // Scrambles the plain text into a secure base64 string
    string Encrypt(string plainText);
    
    // Unscrambles the secure base64 string back to plain text
    string Decrypt(string cipherText);
}