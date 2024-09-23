using Blog.Core.Models.Base;
using Blog.Core.Models.Report;

namespace Blog.Core.Services
{
    public interface IRoyaltyService
    {
        Task<PagingResponse<RoyaltyReportResponse>> GetRoyaltyReportAsync(string? username,
             string fromDate, string toDate, int pageIndex = 1, int pageSize = 10);
        Task PayRoyaltyForUserAsync(Guid fromUserId, Guid toUserId);
    }
}
