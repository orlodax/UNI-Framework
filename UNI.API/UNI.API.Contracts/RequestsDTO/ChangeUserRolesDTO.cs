using System.Collections.Generic;
using UNI.API.Contracts.Models;

namespace UNI.API.Contracts.RequestsDTO
{
    public class ChangeUserRolesDTO
    {
        public string Username { get; set; }
        public IEnumerable<Role> Roles { get; set; }
    }
}
