using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;
using StargateAPI.Controllers;

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

            var person = context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is not null) throw new BadHttpRequestException("Bad Request");
        }
    }

    public sealed class CreatePersonHandler(IDbContextFactory<StargateContext> contextFactory)
        : IRequestHandler<CreatePerson, Result<CreatePersonResult, Exception>>
    {
        public async Task<Result<CreatePersonResult, Exception>> Handle(CreatePerson request,
            CancellationToken cancellationToken)
        {
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

                return Result<CreatePersonResult, Exception>.Ok(result);
            }
            catch (Exception ex)
            {
                return Result<CreatePersonResult, Exception>.Err(ex);
            }
        }
    }

    public sealed class CreatePersonResult
    {
        public int Id { get; set; }
    }
}