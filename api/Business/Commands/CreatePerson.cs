using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;
using StargateAPI.Controllers;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Commands
{
    public sealed class CreatePerson : IRequest<Result<CreatePersonResult, Exception>>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public sealed class CreatePersonPreProcessor(IDbContextFactory<StargateContext> contextFactory)
        : IRequestPreProcessor<CreatePerson>
    {
        public async Task Process(CreatePerson request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var person = await context.People.AsNoTracking().FirstOrDefaultAsync(z => z.Name == request.Name, cancellationToken);

            if (person is not null) throw new BadHttpRequestException("Person already exists");
        }
    }

    public sealed partial class CreatePersonHandler(IDbContextFactory<StargateContext> contextFactory, ILogger<CreatePersonHandler> logger)
        : IRequestHandler<CreatePerson, Result<CreatePersonResult, Exception>>
    {
        public async Task<Result<CreatePersonResult, Exception>> Handle(CreatePerson request,
            CancellationToken cancellationToken)
        {
            LogExecutingCreatePerson(logger, request.Name);
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

                var newPerson = new Person()
                {
                    Name = request.Name
                };

                await context.ExecuteInTransactionAsync<bool>(async (ctx) =>
                {
                    await context.People.AddAsync(newPerson, cancellationToken);
                    return true;
                }, cancellationToken: cancellationToken);

                var result = new CreatePersonResult()
                {
                    Id = newPerson.Id
                };

                LogCreatePersonSuccess(logger, request.Name, result.Id);
                return Result<CreatePersonResult, Exception>.Ok(result);
            }
            catch (Exception ex)
            {
                LogCreatePersonError(logger, request.Name, ex);
                return Result<CreatePersonResult, Exception>.Err(ex);
            }
        }

        [LoggerMessage(LogLevel.Information, "Executing CreatePerson command for name: {Name}")]
        private static partial void LogExecutingCreatePerson(ILogger logger, string name);

        [LoggerMessage(LogLevel.Information, "CreatePerson command completed successfully for name: {Name}. Created person with ID: {Id}")]
        private static partial void LogCreatePersonSuccess(ILogger logger, string name, int id);

        [LoggerMessage(LogLevel.Error, "Error executing CreatePerson command for name: {Name}")]
        private static partial void LogCreatePersonError(ILogger logger, string name, Exception exception);
    }

    public sealed class CreatePersonResult
    {
        public int Id { get; set; }
    }
}