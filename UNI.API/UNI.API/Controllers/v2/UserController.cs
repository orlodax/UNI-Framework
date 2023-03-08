using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNI.API.Contracts.Models;

namespace UNI.API.Controllers.v2;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class UserController : GenericControllerV2<User>
{
    public UserController(ILogger<User> logger, IConfiguration configuration) : base(logger, configuration)
    {
        dbContext = new(configuration.GetConnectionString("IdentityDb")!);
    }
}

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class RoleController : GenericControllerV2<Role>
{
    public RoleController(ILogger<Role> logger, IConfiguration configuration) : base(logger, configuration)
    {
        dbContext = new(configuration.GetConnectionString("IdentityDb")!);
    }
}