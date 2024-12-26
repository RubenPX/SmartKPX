using DotNetEnv;
using SmartAPX.Routes;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddCors();

// Add services to the container.
var app = builder.Build();

#if DEBUG
app.UseCors((c) => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
#endif

// Configure the HTTP request pipeline.
app.AddTuyaEndpoints();

app.Run();