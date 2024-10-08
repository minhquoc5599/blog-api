﻿using AutoMapper;
using Blog.Core.Domain.Report;
using Blog.Core.Models.Base;
using Blog.Core.Models.Report;
using Blog.Core.Repositories;
using Blog.Data.SeedWorks;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Repositories
{
    public class TransactionRepository : RepositoryBase<Transaction, Guid>, ITransactionRepository
    {
        private readonly IMapper _mapper;
        public TransactionRepository(BlogContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<PagingResponse<TransactionResponse>> GetTransactions(string? userName,
            DateTime fromDate, DateTime toDate, int pageIndex = 1, int pageSize = 10)
        {
            var query = _context.Transactions.AsQueryable();
            if (!string.IsNullOrWhiteSpace(userName))
            {
                query = query.Where(x =>
                x.ToUserName.Contains(userName) &&
                x.DateCreated >= fromDate &&
                x.DateCreated <= toDate.AddDays(1));
            }

            var totalRow = await query.CountAsync();
            query = query.OrderByDescending(x => x.DateCreated)
               .Skip((pageIndex - 1) * pageSize)
               .Take(pageSize);
            return new PagingResponse<TransactionResponse>
            {
                Results = await _mapper.ProjectTo<TransactionResponse>(query).ToListAsync(),
                CurrentPage = pageIndex,
                RowCount = totalRow,
                PageSize = pageSize
            };
        }
    }
}
