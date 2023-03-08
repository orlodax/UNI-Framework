using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNI.API.Contracts.Models;
using UNI.API.Contracts.RequestsDTO;
using UNI.API.Core.Services;
using UNI.Core.Library;

namespace UNI.API.Controllers.v2;

[ApiController]
[Route("api/v{version:apiVersion}/identity")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class IdentityController : Controller
{
    private readonly ILogger logger;
    private readonly IdentityService identityService;

    private const uint passwordLifetime = 30;

    public IdentityController(IConfiguration configuration)
    {
        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                                                    .SetMinimumLevel(LogLevel.Trace)
                                                    .AddConsole());
        logger = loggerFactory.CreateLogger<IdentityController>();

        identityService = new(configuration);
    }

    /// <summary>
    /// Clients will call this endpoint with user data at any time to obtain a valid token
    /// </summary>
    /// <param name="credentials"></param>
    /// <returns></returns>
    [HttpPost("token")]
    public IActionResult GetToken([FromBody] Credentials credentials)
    {
        if (identityService.AreCredentialsValid(credentials.Username, credentials.Password, out Credentials? user) && user != null)
        {
            if (user.LastModify < DateTime.Now.AddDays(-passwordLifetime))
                return Unauthorized("Password expired. Change password and authenticate again.");

            UNIToken token = identityService.GenerateToken(user, passwordLifetime);
            logger.Log(LogLevel.Information, "{controllerName}: User '{userName}' requested a token.", nameof(IdentityController), credentials.Username);

            return new ObjectResult(token);
        }

        return Unauthorized("Wrong username or password");
    }

    /// <summary>
    /// Authenticated users/clients (they have token above) can call this to change their password at any time
    /// </summary>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    [HttpPost("changePassword")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult ChangePassword([FromBody] ChangePasswordRequestDTO requestDTO)
    {
        if (HttpContext.User.Identities.First().Name != requestDTO.Username)
            return Unauthorized("Wrong username");

        if (!identityService.AreCredentialsValid(requestDTO.Username, requestDTO.OldPassword, out Credentials? user) || user == null)
            return Unauthorized("Wrong username or password");

        identityService.ChangePassword(requestDTO.Username, requestDTO.NewPassword);
        logger.Log(LogLevel.Information, "{controllerName}: User '{userName}' changed their password.", nameof(IdentityController), requestDTO.Username);

        return Ok();
    }

    #region Users and Roles admin

  
    [HttpPost("admin/resetPassword")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public IActionResult ResetPassword([FromBody] Credentials newCredentials)
    {
        identityService.ChangePassword(newCredentials.Username, newCredentials.Password);
        logger.Log(LogLevel.Information, "{controllerName}: Admin '{adminName}' reset the password of user '{userName}'.", nameof(IdentityController), HttpContext.User.Identities.First().Name, newCredentials.Username);

        return Ok();
    }
    #endregion
}