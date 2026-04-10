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
    public sealed class GetAstronautDutiesByName : IRequest<Result<GetAstronautDutiesByNameResult, Exception>>
    {
        public string Name { get; set; } = string.Empty;
    }

    public sealed partial class GetAstronautDutiesByNameHandler(IDbContextFactory<StargateContext> contextFactory, ILogger<GetAstronautDutiesByNameHandler> logger)
        : IRequestHandler<GetAstronautDutiesByName, Result<GetAstronautDutiesByNameResult, Exception>>
    {
        public async Task<Result<GetAstronautDutiesByNameResult, Exception>> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
            LogExecutingGetAstronautDutiesByName(logger, request.Name);
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
                
                var query = $"SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE \'{request.Name}\' = a.Name";

                var person = await context.Connection.QueryFirstOrDefaultAsync<PersonAstronaut>(query);

                query = $"SELECT * FROM [AstronautDuty] WHERE {person.PersonId} = PersonId Order By DutyStartDate Desc";

                var duties = await context.Connection.QueryAsync<AstronautDuty>(query);

                var result = new GetAstronautDutiesByNameResult
                {
                    Person = person,
                    AstronautDuties = duties.ToList()
                };

                LogGetAstronautDutiesByNameSuccess(logger, request.Name, result.AstronautDuties.Count);
                return Result<GetAstronautDutiesByNameResult, Exception>.Ok(result);
            }
            catch (Exception ex)
            {
                LogGetAstronautDutiesByNameError(logger, request.Name, ex);
                return Result<GetAstronautDutiesByNameResult, Exception>.Err(ex);
            }
        }

        [LoggerMessage(LogLevel.Information, "Executing GetAstronautDutiesByName query for name: {Name}")]
        private static partial void LogExecutingGetAstronautDutiesByName(ILogger logger, string name);

        [LoggerMessage(LogLevel.Information, "GetAstronautDutiesByName query completed for name: {Name}. Retrieved {Count} duties")]
        private static partial void LogGetAstronautDutiesByNameSuccess(ILogger logger, string name, int count);

        [LoggerMessage(LogLevel.Error, "Error executing GetAstronautDutiesByName query for name: {Name}")]
        private static partial void LogGetAstronautDutiesByNameError(ILogger logger, string name, Exception exception);
    }

    public sealed class GetAstronautDutiesByNameResult
    {
        public PersonAstronaut Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
