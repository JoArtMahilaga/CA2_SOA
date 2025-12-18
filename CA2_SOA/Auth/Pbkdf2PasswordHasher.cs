using System.Security.Cryptography;

namespace CA2SOA.Auth;

public interface IPasswordHasher
{
    (byte[] Hash, byte[] Salt) HashPassword(string password);
    bool Verify(string password, byte[] hash, byte[] salt);
}

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public (byte[] Hash, byte[] Salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return (hash, salt);
    }

    public bool Verify(string password, byte[] hash, byte[] salt)
    {
        if (hash is null || salt is null) return false;
        if (hash.Length != HashSize) return false;

        var candidate = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return CryptographicOperations.FixedTimeEquals(candidate, hash);
    }
}