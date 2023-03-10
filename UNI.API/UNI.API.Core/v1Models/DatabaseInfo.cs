using UNI.Core.Library;

namespace UNI.API.Core.v1Models;

[ClassInfo(SQLName = "databasesInfo")]
public class DatabaseInfo
{
    [ValueInfo(SQLName = "connectionstring")]
    public string Connectionstring { get; set; }

    [ValueInfo(SQLName = "apikey")]
    public string ApiKey { get; set; }
}
