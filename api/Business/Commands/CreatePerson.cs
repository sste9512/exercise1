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

    public sealed class CreatePersonPreProcessor : IRequestPreProcessor<CreatePerson>
    {
        private readonly IDbContextFactory<StargateContext> _contextFactory;
        
        public CreatePersonPreProcessor(IDbContextFactory<StargateContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        
        public async Task Process(CreatePerson request, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            var person = context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is not null) throw new BadHttpRequestException("Bad Request");
        }
    }

    public sealed class CreatePersonHandler : IRequestHandler<CreatePerson, Result<CreatePersonResult, Exception>>
    {
        private readonly IDbContextFactory<StargateContext> _contextFactory;

        public CreatePersonHandler(IDbContextFactory<StargateContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        
        public async Task<Result<CreatePersonResult, Exception>> Handle(CreatePerson request, CancellationToken cancellationToken)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
                
                var newPerson = new Person()
                {
                   Name = request.Name
                };

                await context.People.AddAsync(newPerson, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);

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
