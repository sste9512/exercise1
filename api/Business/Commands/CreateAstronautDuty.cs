using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public sealed class CreateAstronautDuty : IRequest<Result<CreateAstronautDutyResult, Exception>>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }
    }

    public sealed class CreateAstronautDutyPreProcessor(IDbContextFactory<StargateContext> contextFactory)
        : IRequestPreProcessor<CreateAstronautDuty>
    {
        public async Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var person = context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is null) throw new BadHttpRequestException("Bad Request");

            var verifyNoPreviousDuty = context.AstronautDuties.FirstOrDefault(z =>
                z.DutyTitle == request.DutyTitle && z.DutyStartDate == request.DutyStartDate);

            if (verifyNoPreviousDuty is not null) throw new BadHttpRequestException("Bad Request");
        }
    }

    public sealed class CreateAstronautDutyHandler(IDbContextFactory<StargateContext> contextFactory)
        : IRequestHandler<CreateAstronautDuty, Result<CreateAstronautDutyResult, Exception>>
    {
        public async Task<Result<CreateAstronautDutyResult, Exception>> Handle(CreateAstronautDuty request,
            CancellationToken cancellationToken)
        {
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

                var query = $"SELECT * FROM [Person] WHERE \'{request.Name}\' = Name";

                var person = await context.Connection.QueryFirstOrDefaultAsync<Person>(query);

                query = $"SELECT * FROM [AstronautDetail] WHERE {person.Id} = PersonId";

                var astronautDetail = await context.Connection.QueryFirstOrDefaultAsync<AstronautDetail>(query);

                if (astronautDetail == null)
                {
                    astronautDetail = new AstronautDetail
                    {
                        PersonId = person.Id,
                        CurrentDutyTitle = request.DutyTitle,
                        CurrentRank = request.Rank,
                        CareerStartDate = request.DutyStartDate.Date
                    };
                    if (request.DutyTitle == "RETIRED")
                    {
                        astronautDetail.CareerEndDate = request.DutyStartDate.Date;
                    }

                    await context.AstronautDetails.AddAsync(astronautDetail, cancellationToken);
                }
                else
                {
                    astronautDetail.CurrentDutyTitle = request.DutyTitle;
                    astronautDetail.CurrentRank = request.Rank;
                    if (request.DutyTitle == "RETIRED")
                    {
                        astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                    }

                    context.AstronautDetails.Update(astronautDetail);
                }

                query = $"SELECT * FROM [AstronautDuty] WHERE {person.Id} = PersonId Order By DutyStartDate Desc";

                var astronautDuty = await context.Connection.QueryFirstOrDefaultAsync<AstronautDuty>(query);

                if (astronautDuty != null)
                {
                    astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                    context.AstronautDuties.Update(astronautDuty);
                }

                var newAstronautDuty = new AstronautDuty()
                {
                    PersonId = person.Id,
                    Rank = request.Rank,
                    DutyTitle = request.DutyTitle,
                    DutyStartDate = request.DutyStartDate.Date,
                    DutyEndDate = null
                };

                await context.AstronautDuties.AddAsync(newAstronautDuty, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);

                var result = new CreateAstronautDutyResult()
                {
                    Id = newAstronautDuty.Id
                };

                return Result<CreateAstronautDutyResult, Exception>.Ok(result);
            }
            catch (Exception ex)
            {
                return Result<CreateAstronautDutyResult, Exception>.Err(ex);
            }
        }
    }

    public sealed class CreateAstronautDutyResult
    {
        public int? Id { get; set; }
    }
}