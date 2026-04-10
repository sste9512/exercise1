using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Values;
using StargateAPI.Controllers;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Queries
{
    public sealed class GetPeople : IRequest<Result<GetPeopleResult, Exception>>
    {

    }

    public sealed partial class GetPeopleHandler(IDbContextFactory<StargateContext> contextFactory, ILogger<GetPeopleHandler> logger)
        : IRequestHandler<GetPeople, Result<GetPeopleResult, Exception>>
    {
        public async Task<Result<GetPeopleResult, Exception>> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            LogExecutingGetPeople(logger);
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
                
                const string query = @"
                    SELECT 
                        a.Id as PersonId, 
                        a.Name, 
                        b.CurrentRank, 
                        b.CurrentDutyTitle, 
                        b.CareerStartDate, 
                        b.CareerEndDate 
                    FROM [Person] a 
                    LEFT JOIN [AstronautDetail] b ON b.PersonId = a.Id 
                    WHERE a.IsDeleted = 0";

                var people = await context.Connection.QueryAsync<PersonAstronaut>(
                    query,
                    commandTimeout: 30);

                var result = new GetPeopleResult
                {
                    People = people.ToList()
                };

                LogGetPeopleSuccess(logger, result.People.Count);
                return Result<GetPeopleResult, Exception>.Ok(result);
            }
            catch (Exception ex)
            {
                LogGetPeopleError(logger, ex);
                return Result<GetPeopleResult, Exception>.Err(ex);
            }
        }

        [LoggerMessage(LogLevel.Information, "Executing GetPeople query")]
        private static partial void LogExecutingGetPeople(ILogger logger);

        [LoggerMessage(LogLevel.Information, "GetPeople query completed successfully. Retrieved {Count} people")]
        private static partial void LogGetPeopleSuccess(ILogger logger, int count);

        [LoggerMessage(LogLevel.Error, "Error executing GetPeople query")]
        private static partial void LogGetPeopleError(ILogger logger, Exception exception);
    }

    public sealed class GetPeopleResult
    {
        public List<PersonAstronaut> People { get; set; } = new List<PersonAstronaut> { };
    }
}
