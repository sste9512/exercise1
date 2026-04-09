using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Values;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public sealed class GetPeople : IRequest<Result<GetPeopleResult, Exception>>
    {

    }

    public sealed class GetPeopleHandler(IDbContextFactory<StargateContext> contextFactory)
        : IRequestHandler<GetPeople, Result<GetPeopleResult, Exception>>
    {
        public async Task<Result<GetPeopleResult, Exception>> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
                
                var query = $"SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id";

                var people = await context.Connection.QueryAsync<PersonAstronaut>(query);

                var result = new GetPeopleResult
                {
                    People = people.ToList()
                };

                return Result<GetPeopleResult, Exception>.Ok(result);
            }
            catch (Exception ex)
            {
                return Result<GetPeopleResult, Exception>.Err(ex);
            }
        }
    }

    public sealed class GetPeopleResult
    {
        public List<PersonAstronaut> People { get; set; } = new List<PersonAstronaut> { };
    }
}
