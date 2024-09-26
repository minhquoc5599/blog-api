using Blog.Core.Repositories;

namespace Blog.Core.SeedWorks
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IPostRepository Posts { get; }
        IPostCategoryRepository PostCategories { get; }
        ISeriesRepository Series { get; }
        ITransactionRepository Transactions { get; }
        Task<int> CompleteAsync();
    }
}
