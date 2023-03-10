using UNI.Core.Library;

namespace UNI.API.Core.v1Models;

[ClassInfo(SQLName = "users")]
public class ApiUser
{
    [ValueInfo(SQLName = "username")]
    public string Username { get; set; }

    [ValueInfo(SQLName = "password")]
    public string Password { get; set; }

    [ValueInfo(SQLName = "idcompany")]
    public int IdCompany { get; set; }

    [ValueInfo()]
    public ApiCompany Company { get; set; }
}
