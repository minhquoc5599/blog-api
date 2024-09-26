using AutoMapper;
using Blog.Core.Domain.Identity;
using Blog.Core.Repositories;
using Blog.Core.SeedWorks;
using Blog.Data.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Blog.Data.SeedWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly BlogContext _context;
        public UnitOfWork(BlogContext context, IMapper mapper, UserManager<AppUser> userManager)
        {
            _context = context;
            Users = new UserRepository(context); 
            Posts = new PostRepository(context, mapper, userManager);
            PostCategories = new PostCategoryRepository(context, mapper);
            Series = new SeriesRepository(context, mapper);
            Transactions = new TransactionRepository(context, mapper);
        }

        public IUserRepository Users { get; private set; }
        public IPostRepository Posts { get; private set; }
        public IPostCategoryRepository PostCategories { get; private set; }
        public ISeriesRepository Series { get; private set; }
        public ITransactionRepository Transactions { get; private set; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();

        }
    }
}
