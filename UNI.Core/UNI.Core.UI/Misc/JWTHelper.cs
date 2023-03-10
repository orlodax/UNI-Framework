using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UNI.API.Client;

namespace UNI.Core.UI.Misc
{
    public static class JWTHelper
    {
        public static bool IsTokenValid()
        {
            if (UNIUser.Token != null)
            {
                JwtSecurityToken jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(UNIUser.Token.Value);
                if (jwtSecurityToken.ValidTo > DateTime.UtcNow)
                    return true;
            }

            return false;
        }

        public static IEnumerable<string> GetUserRolesFromToken()
        {
            JwtSecurityToken jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(UNIUser.Token?.Value);
            foreach (Claim claim in jwtSecurityToken.Claims)
                if (claim.Type == ClaimTypes.Role)
                    yield return claim.Value;
        }

        public static bool IsCurrentUserInRole(string role)
        {
            JwtSecurityToken jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(UNIUser.Token?.Value);
            foreach (Claim claim in jwtSecurityToken.Claims)
                if (claim.Type == ClaimTypes.Role)
                    if (claim.Value == role)
                        return true;

            return false;
        }
    }

}
