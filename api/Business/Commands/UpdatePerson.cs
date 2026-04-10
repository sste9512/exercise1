using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Commands
{
    public sealed class UpdatePerson : IRequest<Result<UpdatePersonResult, Exception>>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public sealed partial class UpdatePersonHandler(IDbContextFactory<StargateContext> contextFactory, ILogger<UpdatePersonHandler> logger)
        : IRequestHandler<UpdatePerson, Result<UpdatePersonResult, Exception>>
    {
        public async Task<Result<UpdatePersonResult, Exception>> Handle(UpdatePerson request,
            CancellationToken cancellationToken)
        {
            LogExecutingUpdatePerson(logger, request.Name);
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

                var person = await context.People.FirstOrDefaultAsync(z => z.Name == request.Name, cancellationToken);

                if (person is null)
                {
                    throw new BadHttpRequestException("Person not found");
                }

                await context.ExecuteInTransactionAsync<bool>(async (ctx) =>
                {
                    return true;
                }, cancellationToken: cancellationToken);

                var result = new UpdatePersonResult()
                {
                    Id = person.Id
                };

                LogUpdatePersonSuccess(logger, request.Name, result.Id);
                return Result<UpdatePersonResult, Exception>.Ok(result);
            }
            catch (Exception ex)
            {
                LogUpdatePersonError(logger, request.Name, ex);
                return Result<UpdatePersonResult, Exception>.Err(ex);
            }
        }

        [LoggerMessage(LogLevel.Information, "Executing UpdatePerson command for name: {Name}")]
        private static partial void LogExecutingUpdatePerson(ILogger logger, string name);

        [LoggerMessage(LogLevel.Information, "UpdatePerson command completed successfully for name: {Name}. Updated person with ID: {Id}")]
        private static partial void LogUpdatePersonSuccess(ILogger logger, string name, int id);

        [LoggerMessage(LogLevel.Error, "Error executing UpdatePerson command for name: {Name}")]
        private static partial void LogUpdatePersonError(ILogger logger, string name, Exception exception);
    }

    public sealed class UpdatePersonResult
    {
        public int Id { get; set; }
    }
}
