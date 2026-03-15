using System.Security.Cryptography;

namespace Fic.MerchantAccounts;

public static class MerchantPasswordHasher
{
    private const int SaltLength = 16;
    private const int HashLength = 32;
    private const int Iterations = 100_000;

    public static (string PasswordHash, string PasswordSalt) HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            throw new InvalidOperationException("Merchant password must be at least 8 characters.");
        }

        Span<byte> salt = stackalloc byte[SaltLength];
        RandomNumberGenerator.Fill(salt);

        Span<byte> hash = stackalloc byte[HashLength];
        Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            hash,
            Iterations,
            HashAlgorithmName.SHA256);

        return (
            Convert.ToBase64String(hash),
            Convert.ToBase64String(salt));
    }

    public static bool VerifyPassword(string password, string? passwordHash, string? passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || string.IsNullOrWhiteSpace(passwordSalt))
        {
            return false;
        }

        byte[] hash;
        byte[] salt;

        try
        {
            hash = Convert.FromBase64String(passwordHash);
            salt = Convert.FromBase64String(passwordSalt);
        }
        catch (FormatException)
        {
            return false;
        }

        var candidate = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            hash.Length);

        return CryptographicOperations.FixedTimeEquals(candidate, hash);
    }
}
