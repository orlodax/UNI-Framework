using System.Security.Cryptography;

namespace UNI.API.Core.Helpers;

public static class PasswordHelper
{
    // 24 byte * 8 = 192 bit
    private const int SaltByteLength = 24;
    private const int DerivedKeyLength = 24;

    // whatever number is appropriate for the current age  
    private const int IterationCount = 24000;

    public static string CreatePasswordHash(string password)
    {
        var salt = GenerateRandomSalt();
        var hashValue = GenerateHashValue(password, salt, IterationCount);
        var iterationCountBtyeArr = BitConverter.GetBytes(IterationCount);
        var valueToSave = new byte[SaltByteLength + DerivedKeyLength + iterationCountBtyeArr.Length];
        Buffer.BlockCopy(salt, 0, valueToSave, 0, SaltByteLength);
        Buffer.BlockCopy(hashValue, 0, valueToSave, SaltByteLength, DerivedKeyLength);
        Buffer.BlockCopy(iterationCountBtyeArr, 0, valueToSave, salt.Length + hashValue.Length, iterationCountBtyeArr.Length);
        return Convert.ToBase64String(valueToSave);
    }
    private static byte[] GenerateRandomSalt()
    {
        var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltByteLength];
        rng.GetBytes(salt);
        return salt;
    }
    private static byte[] GenerateHashValue(string password, byte[] salt, int iterationCount)
    {
        using Rfc2898DeriveBytes pbkdf2 = new(password, salt, iterationCount);
        return pbkdf2.GetBytes(DerivedKeyLength);
    }

    public static bool VerifyPassword(string passwordGuess, string? actualSavedHashResults)
    {
        if (string.IsNullOrEmpty(actualSavedHashResults))
            return false;

        //ingredient #1: password salt byte array
        var salt = new byte[SaltByteLength];

        //ingredient #2: byte array of password
        var actualPasswordByteArr = new byte[DerivedKeyLength];

        //convert actualSavedHashResults to byte array
        var actualSavedHashResultsBtyeArr = Convert.FromBase64String(actualSavedHashResults);

        //ingredient #3: iteration count
        var iterationCountLength = actualSavedHashResultsBtyeArr.Length - (salt.Length + actualPasswordByteArr.Length);
        var iterationCountByteArr = new byte[iterationCountLength];
        Buffer.BlockCopy(actualSavedHashResultsBtyeArr, 0, salt, 0, SaltByteLength);
        Buffer.BlockCopy(actualSavedHashResultsBtyeArr, SaltByteLength, actualPasswordByteArr, 0, actualPasswordByteArr.Length);
        Buffer.BlockCopy(actualSavedHashResultsBtyeArr, salt.Length + actualPasswordByteArr.Length, iterationCountByteArr, 0, iterationCountLength);
        var passwordGuessByteArr = GenerateHashValue(passwordGuess, salt, BitConverter.ToInt32(iterationCountByteArr, 0));
        return ConstantTimeComparison(passwordGuessByteArr, actualPasswordByteArr);
    }

    // XOR operator ^ essentially asking the question “are you not equal” on each byte
    private static bool ConstantTimeComparison(byte[] passwordGuess, byte[] actualPassword)
    {
        uint difference = (uint)passwordGuess.Length ^ (uint)actualPassword.Length;

        for (var i = 0; i < passwordGuess.Length && i < actualPassword.Length; i++)
            difference |= (uint)(passwordGuess[i] ^ actualPassword[i]);

        return difference == 0;
    }
}