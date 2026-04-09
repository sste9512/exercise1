using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;

var builder = WebApplication.CreateBuilder(args);

 

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<AuditSaveChangesInterceptor>();
builder.Services.AddPooledDbContextFactory<StargateContext>((serviceProvider, options) => 
    options.UseSqlite(builder.Configuration.GetConnectionString("StarbaseApiDatabase"))
           .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>())
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)), 50);

builder.Services.AddMediatR(cfg =>
{
    cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(StargateAPI.Business.Pipeline.RetryPipelineBehavior<,>));
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

// Register a generic MediatR exception handler to map exceptions to Result.Err
builder.Services.AddTransient(typeof(MediatR.Pipeline.IRequestExceptionHandler<,,>),
    typeof(StargateAPI.Business.Pipeline.GenericExceptionHandler<,,>));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<StargateContext>>();
    await using var context = await contextFactory.CreateDbContextAsync();
    await context.Database.MigrateAsync();
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


