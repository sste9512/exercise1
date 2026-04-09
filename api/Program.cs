using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Stargate API";
    config.Version = "v1";
    config.Description = "API for managing astronauts and their duties";
});

builder.Services.AddSingleton<AuditSaveChangesInterceptor>();
builder.Services.AddPooledDbContextFactory<StargateContext>((serviceProvider, options) => 
    options.UseSqlite(builder.Configuration.GetConnectionString("StarbaseApiDatabase"))
           .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()), 50);

builder.Services.AddMediatR(cfg =>
{
    cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


