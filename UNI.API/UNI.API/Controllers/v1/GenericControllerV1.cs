using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using UNI.API.Core.v1Models;
using UNI.API.DAL.v1;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;

namespace UNI.API.Controllers.v1;

[ApiController]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0", Deprecated = true)]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class GenericControllerV1<T> : Controller where T : BaseModel
{
    private readonly ILogger<T> logger;
    protected DbContextV1 dbContext;

    public GenericControllerV1(ILogger<T> logger, IConfiguration configuration)
    {
        this.logger = logger;
        dbContext = new(configuration.GetConnectionString("MainDb")!);
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<ApiResponseModel<T>>> Get([FromQuery] int? id = null, [FromQuery] string? idName = null, [FromQuery] int? requestedEntriesNumber = null, [FromQuery] int? blockToReturn = null, [FromQuery] string? filterText = null, [FromQuery(Name = "filterExpression")] List<string>? filterExpression = null, [FromQuery] string filterDateFormat = "%Y-%m-%d")
    {
        logger.Log(LogLevel.Information, "GenericControllerV1: Get/v1 was hit");
        try
        {
            var response = dbContext.Get<T>(idToMatch: id, idName: idName, requestedEntriesNumber: requestedEntriesNumber, blockToReturn: blockToReturn, filterText: filterText, filterExpressions: GenericControllerV1<T>.ConvertExpressions(filterExpression), filterDateFormat: filterDateFormat);
            if (response == null)
                return StatusCode(500);
            return response;
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }

    [HttpGet("skiplist")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<T>>> GetAllSkipList()
    {
        logger.Log(LogLevel.Information, "GenericControllerV1: GetAllSkipList was hit");
        return dbContext.SelectObjects<T>();
    }

    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<T>>> GetById(int id)
    {
        logger.Log(LogLevel.Information, "GenericControllerV1: GetById was hit");
        return dbContext.SelectObjects<T>(id);
    }

    [HttpGet("GetWhere")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<T>>> GetWhere([FromQuery] int id, [FromQuery] string idName)
    {
        logger.Log(LogLevel.Information, "GenericControllerV1: GetWhere was hit");
        return dbContext.SelectObjects<T>(id, idName: idName);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<T>> Post([FromBody] T obj)
    {
        logger.Log(LogLevel.Information, "GenericControllerV1: Post was hit");
        int id = await dbContext.InsertViewObject(obj);
        if (id > 0)
        {
            List<T> items = dbContext.SelectObjects<T>(id) ?? new List<T>();
            if (items.Any())
                return items.First();
            else
                return StatusCode(501);
        }
        else return StatusCode(500);
    }

    [HttpPost("List")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> PostList([FromBody] List<T> objs)
    {
        logger.Log(LogLevel.Information, "GenericControllerV1: PostList was hit");
        foreach (T obj in objs)
            await dbContext.InsertViewObject(obj);

        return Ok();
    }

    [HttpPut("List")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> PutList([FromBody] List<T> objs)
    {
        logger.Log(LogLevel.Information, "GenericControllerV1: PutList was hit");
        foreach (var obj in objs)
            await dbContext.UpdateObject(obj);

        return Ok();
    }

    [HttpPost("PostRapid")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<T>> PostRapid([FromBody] T obj)
    {
        logger.Log(LogLevel.Information, "GenericControllerV1: PostRapid was hit");
        int id = await dbContext.InsertRapidObject(obj);
        if (id > 0)
        {
            List<T> items = dbContext.SelectObjects<T>(id) ?? new List<T>();
            if (items.Any())
                return items.First();
            else
                return StatusCode(500);
        }
        else return StatusCode(500);
    }

    [HttpPut]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> Put([FromBody] T obj)
    {
        logger.Log(LogLevel.Information, "GenericControllerV1: Put was hit");
        if (await dbContext.UpdateObject(obj))
            return Ok();
        else
            return StatusCode(500);
    }

    [HttpDelete]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> Delete([FromBody] int id)
    {
        logger.Log(LogLevel.Information, "GenericControllerV1: Delete was hit");
        dbContext.DeleteObject(typeof(T), id);
        return Ok();
    }

    private static List<FilterExpression> ConvertExpressions(List<string> filterExpressions)
    {
        List<FilterExpression> result = new();

        if (filterExpressions != null)
            foreach (string expression in filterExpressions)
                if (!string.IsNullOrWhiteSpace(expression))
                    if (expression.Contains(','))
                    {
                        string[] parts = expression.Split(',');
                        if (parts.Length > 1)
                            result.Add(new FilterExpression() { PropertyName = parts[0], PropertyValue = parts[1] });
                    }

        return result;
    }
}