using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UNI.API.Contracts.Models;
using UNI.API.Core.Helpers;
using UNI.API.DAL.v2;
using UNI.Core.Library;

namespace UNI.API.Core.Services;

public class IdentityService
{
    private readonly IdentityRepo identityRepo;
    private readonly IConfiguration configuration;

    public IdentityService(IConfiguration configuration)
    {
        identityRepo = new IdentityRepo(configuration);
        this.configuration = configuration;
    }

    public async Task<UNIToken> GenerateToken(Credentials user, uint passwordLifetime)
    {
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Name, user.Username),
            //new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
            new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddHours(12)).ToUnixTimeSeconds().ToString()),
        };

        foreach (Role r in await identityRepo.GetUserRoles(user.ID))
            if (!string.IsNullOrWhiteSpace(r.Name))
                claims.Add(new Claim(ClaimTypes.Role, r.Name));

        string JWTkey = configuration.GetValue<string>("JWTkey")!;

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(JWTkey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
        JwtHeader header = new(credentials);
        JwtPayload payload = new(claims);

        JwtSecurityToken token = new(header, payload);

        await identityRepo.LogAccess(user);

        return new UNIToken()
        {
            Value = new JwtSecurityTokenHandler().WriteToken(token),
            IsPasswordExpiring = DateTime.Now >= user.LastModify.AddDays(passwordLifetime - 7) // warn expiring password 1 week in advance
        };
    }

    /// <summary>
    /// Returns a Credentials object if authentication succeeds, otherwise null
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Credentials?> AreCredentialsValid(string username, string password)
    {
        Credentials? user = await identityRepo.GetUser(username);

        if (user != null)
            if (PasswordHelper.VerifyPassword(password, user.Password))
                return user;

        return null;
    }

    public async Task<int> CreateCredentials(string username, string newPassword)
    {
        string hashPassword = PasswordHelper.CreatePasswordHash(newPassword);
        return await identityRepo.CreateCredentials(username, hashPassword);
    }

    public async Task ChangePassword(string username, string newPassword)
    {
        string hashPassword = PasswordHelper.CreatePasswordHash(newPassword);
        await identityRepo.ChangePassword(username, hashPassword);
    }
}
