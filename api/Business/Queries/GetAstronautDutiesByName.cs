using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Values;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public sealed class GetAstronautDutiesByName : IRequest<Result<GetAstronautDutiesByNameResult, Exception>>
    {
        public string Name { get; set; } = string.Empty;
    }

    public sealed class GetAstronautDutiesByNameHandler(IDbContextFactory<StargateContext> contextFactory)
        : IRequestHandler<GetAstronautDutiesByName, Result<GetAstronautDutiesByNameResult, Exception>>
    {
        public async Task<Result<GetAstronautDutiesByNameResult, Exception>> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
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

                return Result<GetAstronautDutiesByNameResult, Exception>.Ok(result);
            }
            catch (Exception ex)
            {
                return Result<GetAstronautDutiesByNameResult, Exception>.Err(ex);
            }
        }
    }

    public sealed class GetAstronautDutiesByNameResult
    {
        public PersonAstronaut Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
