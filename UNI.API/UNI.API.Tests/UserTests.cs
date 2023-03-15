using Microsoft.Extensions.Configuration;
using UNI.API.Contracts.Models;
using UNI.API.Core.Helpers;
using UNI.API.DAL.v2;

namespace UNI.API.Tests;

[TestClass]
public class UserTests
{
    private readonly IConfiguration configuration;
    //private readonly DbContextV2<User> dbContextUser;
    private readonly DbContextV2<Role> dbContextRole;
    private readonly DbContextV2<Credentials> dbContextCredentials;

    public UserTests()
    {
        configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        //dbContextUser = new(configuration.GetConnectionString("IdentityDb")!);
        dbContextRole = new(configuration.GetConnectionString("IdentityDb")!);
        dbContextCredentials = new(configuration.GetConnectionString("IdentityDb")!);
    }

    [DataTestMethod]
    [DataRow("testUsername", "testPassword")]
    public async Task CreateUser(string username, string password)
    {
        List<Credentials> existingUser = dbContextCredentials.GetData($"SELECT * FROM credentials WHERE username = '{username}'");
        if (!existingUser.Any())
        {
            string hashedPassword = PasswordHelper.CreatePasswordHash(password);
            Credentials newUser = new()
            {
                Username = username,
                Password = hashedPassword
            };
            int res = await dbContextCredentials.InsertObject(newUser);
            Assert.IsFalse(res == 0);
        }
    }

    //[TestMethod]
    //public async Task CheckRolesAreLoadedInUser()
    //{
    //    var response = dbContextUser.Get();
    //    var user = response?.ResponseBaseModels.FirstOrDefault();
    //    Assert.IsNotNull(user);
    //    Assert.IsTrue(user.Roles.Any(r => r.ID == 9) && user.Roles.Any(r => r.ID == 8));
    //}

    [DataTestMethod]
    [DataRow("testRole")]
    public async Task CreateRole(string roleName)
    {
        await dbContextRole.SetData($"INSERT INTO roles (name) VALUES ('{roleName}')");
    }

    [DataTestMethod]
    [DataRow("testUsername", "Admin")]
    [DataRow("testUsername", "DefaultUser")]
    public async Task AssignRoleToUser(string username, string roleName)
    {
        List<Credentials> existingUser = dbContextCredentials.GetData($"SELECT * FROM credentials WHERE username = '{username}'");
        List<Role> existingRole = dbContextRole.GetData($"SELECT * FROM roles WHERE name = '{roleName}'");
        if (existingUser.Any() && existingRole.Any())
            await dbContextRole.SetData($"INSERT INTO userroles (userid, roleid) VALUES({existingUser.First().ID}, {existingRole.First().ID})");
    }

    [DataTestMethod]
    [DataRow("testUsername", "testRole")]
    public async Task RevokeRoleFromUser(string username, string roleName)
    {
        List<Credentials> existingUser = dbContextCredentials.GetData($"SELECT * FROM credentials WHERE username = '{username}'");
        if (existingUser.Any())
            await dbContextCredentials.SetData($"DELETE FROM userroles WHERE userid = {existingUser.First().ID} AND roleid IN (SELECT id FROM roles WHERE name = '{roleName}')");
    }

}
