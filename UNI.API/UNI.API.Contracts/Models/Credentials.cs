using UNI.Core.Library;

namespace UNI.API.Contracts.Models
{
    [ClassInfo(SQLName = "credentials")]
    public class Credentials : BaseModel
    {
        [ValueInfo(SQLName = "username", IsDisplayProperty = true)]
        public string Username { get; set; }

        [ValueInfo(SQLName = "password")]
        public string Password { get; set; }
    }
}