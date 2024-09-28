using Blog.Core.Domain.Identity;
using Blog.Core.Domain.Report;
using Blog.Core.Models.Base;
using Blog.Core.Models.Report;
using Blog.Core.SeedWorks;
using Blog.Core.Services;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Blog.Data.Services
{
    public class RoyaltyService : IRoyaltyService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public RoyaltyService(UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<PagingResponse<RoyaltyReportResponse>> GetRoyaltyReportAsync(string? username,
            DateTime fromDate, DateTime toDate, int pageIndex = 1, int pageSize = 10)
        {
            using SqlConnection connection = new(_configuration.GetConnectionString("DefaultConnection"));
            if (connection.State == ConnectionState.Closed)
            {
                await connection.OpenAsync();
            }

            var coreSql = @"select 
                                    p.AuthorUserId as UserId,
                                    u.UserName as UserName,
                                    sum(case when p.Status = 0 then 1 else 0 end) as NumberOfDraftPosts,
                                    sum(case when p.Status = 1 then 1 else 0 end) as NumberOfWaitingApprovalPosts,
                                    sum(case when p.Status = 2 then 1 else 0 end) as NumberOfRejectedPosts,
                                    sum(case when p.Status = 3 then 1 else 0 end) as NumberOfPublishPosts,
                                    sum(case when p.Status = 3 and p.IsPaid = 1 then 1 else 0 end) as NumberOfPaidPublishPosts,
                                    sum(case when p.Status = 3 and p.IsPaid = 0 then 1 else 0 end) as NumberOfUnpaidPublishPosts
                                    from Posts p join AppUsers u on p.AuthorUserId = u.Id
                                    where (@username is null or u.UserName like '%' + @username + '%') and 
                                            p.DateCreated between @fromDate and DateAdd(day, 1, @toDate)
                                    group by 
                                    p.AuthorUserId,
                                    u.UserName";

            var items = await connection.QueryAsync<RoyaltyReportResponse>(coreSql, new
            {
                username,
                fromDate,
                toDate
            }, null, 120, CommandType.Text);
            var totalRow = items.Count();
            items = items.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            return new PagingResponse<RoyaltyReportResponse>
            {
                Results = items.ToList(),
                CurrentPage = pageIndex,
                RowCount = totalRow,
                PageSize = pageSize
            };
        }

        public async Task PayRoyaltyForUserAsync(Guid fromUserId, Guid toUserId)
        {
            var fromUser = await _userManager.FindByIdAsync(fromUserId.ToString()) ?? throw new Exception($"User {fromUserId} not found");
            var toUser = await _userManager.FindByIdAsync(toUserId.ToString()) ?? throw new Exception($"User {toUserId} not found");

            var unpaidPublishPosts = await _unitOfWork.Posts.GetListUnpaidPublishPosts(toUserId);
            double totalRoyalty = 0;
            foreach (var post in unpaidPublishPosts)
            {
                post.IsPaid = true;
                post.PaidDate = DateTime.Now;
                post.RoyaltyAmount = toUser.RoyaltyAmountPerPost;
                totalRoyalty += toUser.RoyaltyAmountPerPost;
            }
            toUser.Balance += totalRoyalty;

            await _userManager.UpdateAsync(toUser);
            _unitOfWork.Transactions.Add(new Transaction()
            {
                FromUserId = fromUser.Id,
                FromUserName = fromUser.UserName,
                ToUserId = toUserId,
                ToUserName = toUser.UserName,
                Amount = totalRoyalty,
                TransactionType = TransactionType.RoyaltyPay,
                Note = $"{fromUser.UserName} pay royalty for {toUser.UserName}"
            });
            await _unitOfWork.CompleteAsync();
        }
    }
}

