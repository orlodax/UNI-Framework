// This is a sample instance of a UNI web API
// For debug purposes only
// Any implementation will have the following code in Program.cs

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
UNI.API.Startup startup = new(builder.Configuration);
startup.ConfigureServices(builder.Services);
WebApplication app = builder.Build();
startup.Configure(app, builder.Environment);