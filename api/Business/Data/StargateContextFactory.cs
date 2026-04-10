using Microsoft.EntityFrameworkCore;

namespace StargateAPI.Business.Data
{
    public sealed class StargateContextFactory : IDbContextFactory<StargateContext>
    {
        private readonly DbContextOptions<StargateContext> _options;

        public StargateContextFactory(DbContextOptions<StargateContext> options)
        {
            _options = options;
        }

        public StargateContext CreateDbContext()
        {
            return new StargateContext(_options);
        }
    }
}
