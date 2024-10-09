using Blog.Core.Repositories;

namespace Blog.Core.SeedWorks
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IPostRepository Posts { get; }
        IPostCategoryRepository PostCategories { get; }
        ISeriesRepository Series { get; }
        ITagRepository Tags { get; }
        ITransactionRepository Transactions { get; }
        Task<int> CompleteAsync();
    }
}
