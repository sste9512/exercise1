using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Values;
using System;

namespace StargateAPI.Business.Queries
{
    public sealed class GetPersonByName : IRequest<Result<GetPersonByNameResult, Exception>>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public sealed class GetPersonByNameHandler : IRequestHandler<GetPersonByName, Result<GetPersonByNameResult, Exception>>
    {
        private readonly IDbContextFactory<StargateContext> _contextFactory;
        
        public GetPersonByNameHandler(IDbContextFactory<StargateContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Result<GetPersonByNameResult, Exception>> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
                
                var query = $"SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE '{request.Name}' = a.Name";

                var person = await context.Connection.QueryAsync<PersonAstronaut>(query);

                var result = new GetPersonByNameResult
                {
                    Person = person.FirstOrDefault()
                };

                return Result<GetPersonByNameResult, Exception>.Ok(result);
            }
            catch (Exception ex)
            {
                return Result<GetPersonByNameResult, Exception>.Err(ex);
            }
        }
    }

    public sealed class GetPersonByNameResult 
    {
        public PersonAstronaut? Person { get; set; }
    }
}
