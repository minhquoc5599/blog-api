using Blog.Core.Domain.Report;
using Blog.Core.Models.Base;
using Blog.Core.Models.Report;
using Blog.Core.SeedWorks;

namespace Blog.Core.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction, Guid>
    {
        Task<PagingResponse<TransactionResponse>> GetTransactions(string? userName,
            DateTime fromDate, DateTime toDate, int pageIndex = 1, int pageSize = 10);
    }
}
