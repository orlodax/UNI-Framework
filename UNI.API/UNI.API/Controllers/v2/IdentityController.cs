using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
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
    private const uint passwordLifetime = 3000;

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
    public async Task<IActionResult> Authenticate([FromBody] Credentials credentials)
    {
        Credentials? user = await identityService.AreCredentialsValid(credentials.Username, credentials.Password);
        if (user == null)
            return Unauthorized("Wrong username or password");

        //if (user.LastModify < DateTime.Now.AddDays(-passwordLifetime))
            //return Unauthorized("Password expired. Change password and authenticate again.");

        UNIToken token = await identityService.GenerateToken(user, passwordLifetime);
        logger.Log(LogLevel.Information, "{controllerName}: User '{userName}' requested a token.", nameof(IdentityController), credentials.Username);

        return new ObjectResult(token);
    }

    /// <summary>
    /// Authenticated users/clients (they have token above) can call this to change their password at any time
    /// </summary>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    [HttpPost("changePassword")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO requestDTO)
    {
        if (HttpContext.User.Identities.First().Name != requestDTO.Username)
            return Unauthorized("Wrong username");
        
        Credentials? user = await identityService.AreCredentialsValid(requestDTO.Username, requestDTO.OldPassword);
        if (user == null)
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


    [HttpPost("admin/createCredential")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<ActionResult<int>> CreateCredential([FromBody] Credentials newCredentials)
    {
        int idCredential = await identityService.CreateCredentials(newCredentials.Username, newCredentials.Password);
        if (idCredential == 0)
            return BadRequest();


        logger.Log(LogLevel.Information, "{controllerName}: Admin '{adminName}' create user '{userName}'.", nameof(IdentityController), HttpContext.User.Identities.First().Name, newCredentials.Username);

        return idCredential;
    }
    #endregion
}