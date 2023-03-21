using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNI.API.Contracts.Models;
using UNI.API.Core.Helpers;
using UNI.API.DAL.v2;

namespace UNI.API.Tests;

[TestClass]
public class UserTests
{
    private readonly string connectionString;
    private readonly ILogger logger;

    public UserTests()
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        connectionString = configuration.GetConnectionString("IdentityDb")!;

        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                                            .SetMinimumLevel(LogLevel.Trace)
                                            .AddConsole());
        logger = loggerFactory.CreateLogger<UserTests>();
    }

    [DataTestMethod]
    [DataRow("testUsername", "testPassword")]
    public async Task CreateUser(string username, string password)
    {
        List<Credentials> existingUser = await new ListHelperV2<Credentials>(connectionString)
            .GetData($"SELECT * FROM credentials WHERE username = '{username}'");

        if (!existingUser.Any())
        {
            string hashedPassword = PasswordHelper.CreatePasswordHash(password);
            Credentials newUser = new()
            {
                Username = username,
                Password = hashedPassword
            };
            int res = await new DbContextV2<Credentials>(connectionString).InsertObject(newUser);
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
        await DALHelper.SetData($"INSERT INTO roles (name) VALUES ('{roleName}')", connectionString, logger);
    }

    [DataTestMethod]
    [DataRow("testUsername", "Admin")]
    [DataRow("testUsername", "DefaultUser")]
    public async Task AssignRoleToUser(string username, string roleName)
    {
        List<Credentials> existingUser = await new ListHelperV2<Credentials>(connectionString)
            .GetData($"SELECT * FROM credentials WHERE username = '{username}'");

        List<Role> existingRole = await new ListHelperV2<Role>(connectionString)
            .GetData($"SELECT * FROM roles WHERE name = '{roleName}'");

        if (existingUser.Any() && existingRole.Any())
            await DALHelper.SetData($"INSERT INTO userroles (userid, roleid) VALUES({existingUser.First().ID}, {existingRole.First().ID})", connectionString, logger);
    }

    [DataTestMethod]
    [DataRow("testUsername", "testRole")]
    public async Task RevokeRoleFromUser(string username, string roleName)
    {
        List<Credentials> existingUser = await new ListHelperV2<Credentials>(connectionString)
            .GetData($"SELECT * FROM credentials WHERE username = '{username}'");

        if (existingUser.Any())
            await DALHelper.SetData($"DELETE FROM userroles WHERE userid = {existingUser.First().ID} AND roleid IN (SELECT id FROM roles WHERE name = '{roleName}')", connectionString, logger);
    }

}
