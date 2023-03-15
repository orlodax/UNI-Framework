using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNI.API.Contracts.RequestsDTO;
using UNI.API.DAL.v2;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;

namespace UNI.API.Controllers.v2;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
public class GenericControllerV2<T> : Controller where T : BaseModel
{
    private readonly ILogger<T> logger;
    protected DbContextV2<T> dbContext;

    public GenericControllerV2(ILogger<T> logger, IConfiguration configuration)
    {
        this.logger = logger;
        dbContext = new(configuration.GetConnectionString("MainDb")!);
    }

    [HttpPost("get")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<ApiResponseModel<T>>> Get([FromBody] GetDataSetRequestDTO requestDTO)
    {
        logger.Log(LogLevel.Information, "GenericControllerV2: Get/v2 was hit");
        try
        {
            var response = await dbContext.Get(requestDTO);
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
        logger.Log(LogLevel.Information, "GenericControllerV2: GetAllSkipList was hit");
        return await dbContext.SelectObjects();
    }

    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<T>>> GetById(int id)
    {
        logger.Log(LogLevel.Information, "GenericControllerV2: GetById was hit");
        return await dbContext.SelectObjects(id);
    }

    [HttpGet("GetWhere")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<T>>> GetWhere([FromQuery] int id, [FromQuery] string idName)
    {
        logger.Log(LogLevel.Information, "GenericControllerV2: GetWhere was hit");
        return await dbContext.SelectObjects(id, idName: idName);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<T>> Post([FromBody] T obj)
    {
        logger.Log(LogLevel.Information, "GenericControllerV2: Post was hit");
        int id = await dbContext.InsertObject(obj);
        if (id > 0)
        {
            List<T> items = await dbContext.SelectObjects(id);
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
        logger.Log(LogLevel.Information, "GenericControllerV2: PostList was hit");
        foreach (T obj in objs)
            await dbContext.InsertObject(obj);

        return Ok();
    }

    [HttpPut("List")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> PutList([FromBody] List<T> objs)
    {
        logger.Log(LogLevel.Information, "GenericControllerV2: PutList was hit");
        foreach (var obj in objs)
            await dbContext.UpdateObject(obj);

        return Ok();
    }

    [HttpPost("PostRapid")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<T>> PostRapid()
    {
        logger.Log(LogLevel.Information, "GenericControllerV2: PostRapid was hit");
        int id = await dbContext.InsertRapidObject();
        if (id > 0)
        {
            List<T> items = await dbContext.SelectObjects(id);
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
        logger.Log(LogLevel.Information, "GenericControllerV2: Put was hit");
        if (await dbContext.UpdateObject(obj))
            return Ok();
        else
            return StatusCode(500);
    }

    [HttpDelete]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> Delete([FromBody] int id)
    {
        logger.Log(LogLevel.Information, "GenericControllerV2: Delete was hit");
        dbContext.DeleteObject(id);
        return Ok();
    }
}
