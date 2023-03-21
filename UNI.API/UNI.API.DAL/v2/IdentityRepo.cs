using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using UNI.API.Contracts.Models;

namespace UNI.API.DAL.v2;

public class IdentityRepo
{
    private readonly string connectionString;
    private readonly ILogger logger;

    public IdentityRepo(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("IdentityDb")!;

        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                                            .SetMinimumLevel(LogLevel.Trace)
                                            .AddConsole());
        logger = loggerFactory.CreateLogger<IdentityRepo>();
    }

    #region User
    public async Task<Credentials?> GetUser(string username)
    {
        List<Credentials> existingUser = await new ListHelperV2<Credentials>(connectionString)
                                            .GetData($"SELECT * FROM credentials WHERE username = '{username}'");

        return existingUser.FirstOrDefault();
    }

    public void LogAccess(Credentials user)
    {
        string query = "INSERT INTO logaccess (username) VALUES (@username)";
        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@username", MySqlDbType.String) { Value = user.Username }
        };

        _ = DALHelper.SetDataParametrized(query, parameters, connectionString, logger);
    }
    #endregion

    #region Password

    /// <summary>
    /// Used by both the changepassword endpoint for default users and resetpassword endpoint for admins
    /// </summary>
    /// <param name="username"></param>
    /// <param name="newPassword"></param>
    public void ChangePassword(string username, string newPassword)
    {
        string query = "UPDATE credentials SET password = @password WHERE username = @username";
        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@username", MySqlDbType.String) { Value = username },
            new MySqlParameter("@password", MySqlDbType.String) { Value = newPassword }
        };

        _ = DALHelper.SetDataParametrized(query, parameters, connectionString, logger);
    }
    #endregion

    #region Roles
    public async Task<List<Role>> GetUserRoles(int userId)
    {
        return await new ListHelperV2<Role>(connectionString)
                .GetData($@"SELECT DISTINCT r.* FROM roles r 
                                    INNER JOIN userroles ur ON ur.idrole = r.id 
                                    INNER JOIN credentials c ON c.id = ur.iduser 
                                WHERE c.id = {userId}");
    }
    #endregion
}
