using UNI.Core.Library;

namespace UNI.API.Core.v1Models;

[ClassInfo(SQLName = "companies")]
public class ApiCompany
{
    [ValueInfo(SQLName = "password")]
    public string Name { get; set; }

    [ValueInfo(SQLName = "apikey")]
    public string ApiKey { get; set; }

    [ValueInfo(SQLName = "endpoint")]
    public string Endpoint { get; set; }
}
