using StargateAPI.Business.Values;

namespace StargateAPI.Business.Data
{
    public static class StargateContextExtensions
    {
        public static async Task<Result<TResult, Exception>> ExecuteInTransactionAsync<TResult>(
            this StargateContext context,
            Func<StargateContext, Task<TResult>> operation,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await operation(context);
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<TResult, Exception>.Ok(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<TResult, Exception>.Err(ex);
            }
        }

        public static async Task<Result<TResult, Exception>> ExecuteInTransactionAsync<TResult>(
            this StargateContext context,
            Func<StargateContext, CancellationToken, Task<TResult>> operation,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await operation(context, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<TResult, Exception>.Ok(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<TResult, Exception>.Err(ex);
            }
        }

        public static Result<TResult, Exception> ExecuteInTransaction<TResult>(
            this StargateContext context,
            Func<StargateContext, TResult> operation)
        {
            using var transaction = context.Database.BeginTransaction();

            try
            {
                var result = operation(context);
                context.SaveChanges();
                transaction.Commit();

                return Result<TResult, Exception>.Ok(result);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Result<TResult, Exception>.Err(ex);
            }
        }
    }
}
