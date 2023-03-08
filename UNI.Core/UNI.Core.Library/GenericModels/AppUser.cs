using System.Collections.Generic;

namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "users")]
    public class AppUser : BaseModel
    {
        [ValueInfo(SQLName = "username", IsDisplayProperty = true)] public string Username { get; set; }
        [ValueInfo(SQLName = "role")] public string Role { get; set; }
        [ValueInfo(IsReadOnly = true)] public string Password { get; set; } //the password is not present in db but could be set in login to local save
        [ValueInfo()] public List<AppPrivilege> AppPrivileges { get; set; }
    }
}