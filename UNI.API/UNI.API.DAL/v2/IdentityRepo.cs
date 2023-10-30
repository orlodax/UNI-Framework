using Microsoft.Extensions.Configuration;
using MySqlConnector;
using UNI.API.Contracts.Models;
using UNI.API.Contracts.RequestsDTO;
using UNI.Core.Library.GenericModels;

namespace UNI.API.DAL.v2;

public class IdentityRepo
{
    private readonly DbContextV2<User> dbUser;
    private readonly DbContextV2<Credentials> dbCredentials;
    private readonly DbContextV2<Role> dbRoles;

    public IdentityRepo(IConfiguration configuration)
    {
        dbUser = new(configuration.GetConnectionString("IdentityDb")!);
        dbCredentials = new(configuration.GetConnectionString("IdentityDb")!);
        dbRoles = new(configuration.GetConnectionString("IdentityDb")!);
    }

    #region User
    public async Task<Credentials?> GetUser(string username)
    {
        List<Credentials> existingUser = await dbCredentials.GetData($"SELECT * FROM credentials WHERE username = '{username}'");

        return existingUser.FirstOrDefault();
    }

    public async Task<List<User>> GetUsers()
    {
        ApiResponseModel<User>? res = await dbUser.Get(new GetDataSetRequestDTO());
        return res?.ResponseBaseModels ?? new List<User>();
    }

    public async Task<int> CreateCredential(string username, string password)
    {
        string query = "INSERT INTO credentials (username, password) VALUES (@username, @password)";
        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@username", MySqlDbType.String) { Value = MySqlHelper.EscapeString(username) },
            new MySqlParameter("@password", MySqlDbType.String) { Value = MySqlHelper.EscapeString(password) }
        };

        _ = dbCredentials.SetDataParametrized(query, parameters);

        query = $"SELECT id FROM credentials WHERE username = '{MySqlHelper.EscapeString(username)}'";
        var credential = await dbCredentials.GetData(query);
        if(credential == null || credential.Count == 0)
            return 0;
        return credential[0].ID;
    }

    public void DeleteUser(string username)
    {
        // foreign key will remove user roles as well

        string query = "DELETE FROM credentials WHERE username = @username";
        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@username", MySqlDbType.String) { Value = username }
        };

        _ = dbCredentials.SetDataParametrized(query, parameters);
    }

    public void LogAccess(Credentials user)
    {
        string query = "INSERT INTO logaccess (username) VALUES (@username)";
        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@username", MySqlDbType.String) { Value = user.Username }
        };

        _ = dbUser.SetDataParametrized(query, parameters);
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

        _ = dbCredentials.SetDataParametrized(query, parameters);
    }
    #endregion

    #region Roles
    public async Task<List<Role>> GetUserRoles(int userId)
    {

        return await dbRoles.GetData($@"SELECT DISTINCT r.* FROM roles r 
                                            INNER JOIN userroles ur ON ur.idrole = r.id 
                                            INNER JOIN credentials c ON c.id = ur.iduser 
                                        WHERE c.id = {userId}");
    }
    #endregion
}
