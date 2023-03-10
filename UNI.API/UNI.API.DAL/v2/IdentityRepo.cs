using Microsoft.Extensions.Configuration;
using MySqlConnector;
using UNI.API.Contracts.Models;
using UNI.API.Contracts.RequestsDTO;

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
    public Credentials? GetUser(string username)
    {
        List<Credentials> existingUser = dbCredentials.GetData($"SELECT * FROM credentials WHERE username = '{username}'");

        return existingUser.FirstOrDefault();
    }

    public List<User> GetUsers()
    {
        return dbUser.Get(new GetDataSetRequestDTO())?.ResponseBaseModels ?? new List<User>();
    }

    public void CreateUser(string username, string password)
    {
        string query = "INSERT INTO credentials (username, password) VALUES (@username, @password)";
        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@username", MySqlDbType.String) { Value = username },
            new MySqlParameter("@password", MySqlDbType.String) { Value = password }
        };

        _ = dbCredentials.SetDataParametrized(query, parameters);
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
    public List<Role> GetUserRoles(int userId)
    {

        return dbRoles.GetData($@"SELECT DISTINCT r.* FROM roles r 
                                            INNER JOIN userroles ur ON ur.idrole = r.id 
                                            INNER JOIN credentials c ON c.id = ur.iduser 
                                        WHERE c.id = {userId}");
    }
    #endregion
}
