using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Values;
using System;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Queries
{
    public sealed class GetPersonByName : IRequest<Result<GetPersonByNameResult, Exception>>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public sealed partial class GetPersonByNameHandler(IDbContextFactory<StargateContext> contextFactory, ILogger<GetPersonByNameHandler> logger)
        : IRequestHandler<GetPersonByName, Result<GetPersonByNameResult, Exception>>
    {
        public async Task<Result<GetPersonByNameResult, Exception>> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            LogExecutingGetPersonByName(logger, request.Name);
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
                
                var query = $"SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE '{request.Name}' = a.Name";

                var person = await context.Connection.QueryAsync<PersonAstronaut>(query);

                var result = new GetPersonByNameResult
                {
                    Person = person.FirstOrDefault()
                };

                LogGetPersonByNameSuccess(logger, request.Name, result.Person != null);
                return Result<GetPersonByNameResult, Exception>.Ok(result);
            }
            catch (Exception ex)
            {
                LogGetPersonByNameError(logger, request.Name, ex);
                return Result<GetPersonByNameResult, Exception>.Err(ex);
            }
        }

        [LoggerMessage(LogLevel.Information, "Executing GetPersonByName query for name: {Name}")]
        private static partial void LogExecutingGetPersonByName(ILogger logger, string name);

        [LoggerMessage(LogLevel.Information, "GetPersonByName query completed for name: {Name}. Found: {Found}")]
        private static partial void LogGetPersonByNameSuccess(ILogger logger, string name, bool found);

        [LoggerMessage(LogLevel.Error, "Error executing GetPersonByName query for name: {Name}")]
        private static partial void LogGetPersonByNameError(ILogger logger, string name, Exception exception);
    }

    public sealed class GetPersonByNameResult 
    {
        public PersonAstronaut? Person { get; set; }
    }
}
