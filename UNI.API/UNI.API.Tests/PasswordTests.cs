using System.Security.Cryptography;
using UNI.API.Core.Helpers;

namespace UNI.API.Tests;

[TestClass]
public class PasswordTests
{
    [TestMethod]
    public void PlainSHA256()
    {
        string password = "@~Some;Password4!";

        // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
        byte[] salt = new byte[128 / 8];
        var rng = RandomNumberGenerator.Create();
        rng.GetNonZeroBytes(salt);

        var finalPassword = salt.Concat(System.Text.Encoding.UTF8.GetBytes(password));

        var sha256 = SHA256.Create();
        sha256.ComputeHash(finalPassword.ToArray());
    }

    [TestMethod]
    public void PBKDF2()
    {
        string password = "SimTek2023.";
        string hash = PasswordHelper.CreatePasswordHash(password);
    }

    [TestMethod]
    public void PBKDF2_Verify()
    {
        string password = "@~Some;Password4!";
        string wrongpassword = "brazorf";
        Assert.IsTrue(PasswordHelper.VerifyPassword(password, PasswordHelper.CreatePasswordHash(password)));
        Assert.IsFalse(PasswordHelper.VerifyPassword(wrongpassword, PasswordHelper.CreatePasswordHash(password)));
    }

    [TestMethod]
    public void StorePassword()
    {

    }
}
