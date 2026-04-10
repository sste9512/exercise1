using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;
using StargateAPI.Controllers;
using System.Net;
using Microsoft.Extensions.Logging;

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

            var person = await context.People.AsNoTracking().FirstOrDefaultAsync(z => z.Name == request.Name, cancellationToken);

            if (person is null) throw new BadHttpRequestException("Person not found");

            var verifyNoPreviousDuty = await context.AstronautDuties.AnyAsync(z =>
                z.PersonId == person.Id && z.DutyTitle == request.DutyTitle && z.DutyStartDate == request.DutyStartDate, cancellationToken);

            if (verifyNoPreviousDuty) throw new BadHttpRequestException("Duty already exists for this person at this start date");
        }
    }

    public sealed partial class CreateAstronautDutyHandler(IDbContextFactory<StargateContext> contextFactory, ILogger<CreateAstronautDutyHandler> logger)
        : IRequestHandler<CreateAstronautDuty, Result<CreateAstronautDutyResult, Exception>>
    {
        public async Task<Result<CreateAstronautDutyResult, Exception>> Handle(CreateAstronautDuty request,
            CancellationToken cancellationToken)
        {
            LogExecutingCreateAstronautDuty(logger, request.Name, request.DutyTitle);
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

                return await context.ExecuteInTransactionAsync(async (ctx, ct) =>
                {
                    var person = await ctx.People
                        .Include(p => p.AstronautDetail)
                        .FirstOrDefaultAsync(p => p.Name == request.Name, ct);

                    if (person == null)
                    {
                        throw new Exception("Person not found");
                    }

                    // Rule 6 & 7: Retired logic
                    if (request.DutyTitle == "RETIRED")
                    {
                        if (person.AstronautDetail == null)
                        {
                            throw new Exception("Cannot retire a person who has never had an astronaut assignment");
                        }
                        
                        person.AstronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                        person.AstronautDetail.CurrentDutyTitle = request.DutyTitle;
                        person.AstronautDetail.CurrentRank = request.Rank;
                        ctx.AstronautDetails.Update(person.AstronautDetail);
                    }
                    else
                    {
                        // Update or Create AstronautDetail
                        if (person.AstronautDetail == null)
                        {
                            person.AstronautDetail = new AstronautDetail
                            {
                                PersonId = person.Id,
                                CurrentDutyTitle = request.DutyTitle,
                                CurrentRank = request.Rank,
                                CareerStartDate = request.DutyStartDate.Date
                            };
                            await ctx.AstronautDetails.AddAsync(person.AstronautDetail, ct);
                        }
                        else
                        {
                            person.AstronautDetail.CurrentDutyTitle = request.DutyTitle;
                            person.AstronautDetail.CurrentRank = request.Rank;
                            ctx.AstronautDetails.Update(person.AstronautDetail);
                        }
                    }

                    // Rule 5: Set Previous Duty End Date
                    var previousDuties = await ctx.AstronautDuties
                        .Where(d => d.PersonId == person.Id && d.DutyEndDate == null)
                        .ToListAsync(ct);

                    foreach (var duty in previousDuties)
                    {
                        duty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                        ctx.AstronautDuties.Update(duty);
                    }

                    // Rule 4: New current duty has no Duty End Date
                    var newAstronautDuty = new AstronautDuty()
                    {
                        PersonId = person.Id,
                        Rank = request.Rank,
                        DutyTitle = request.DutyTitle,
                        DutyStartDate = request.DutyStartDate.Date,
                        DutyEndDate = null
                    };

                    await ctx.AstronautDuties.AddAsync(newAstronautDuty, ct);

                    var result = new CreateAstronautDutyResult()
                    {
                        Id = newAstronautDuty.Id
                    };

                    LogCreateAstronautDutySuccess(logger, request.Name, request.DutyTitle, result.Id ?? 0);
                    return result;
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                LogCreateAstronautDutyError(logger, request.Name, request.DutyTitle, ex);
                return Result<CreateAstronautDutyResult, Exception>.Err(ex);
            }
        }

        [LoggerMessage(LogLevel.Information, "Executing CreateAstronautDuty command for name: {Name}, duty: {DutyTitle}")]
        private static partial void LogExecutingCreateAstronautDuty(ILogger logger, string name, string dutyTitle);

        [LoggerMessage(LogLevel.Information, "CreateAstronautDuty command completed successfully for name: {Name}, duty: {DutyTitle}. Created duty with ID: {Id}")]
        private static partial void LogCreateAstronautDutySuccess(ILogger logger, string name, string dutyTitle, int id);

        [LoggerMessage(LogLevel.Error, "Error executing CreateAstronautDuty command for name: {Name}, duty: {DutyTitle}")]
        private static partial void LogCreateAstronautDutyError(ILogger logger, string name, string dutyTitle, Exception exception);
    }

    public sealed class CreateAstronautDutyResult
    {
        public int? Id { get; set; }
    }
}