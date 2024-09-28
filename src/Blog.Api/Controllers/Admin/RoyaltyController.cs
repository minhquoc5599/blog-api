using Blog.Api.Helps.Extensions;
using Blog.Core.Models.Base;
using Blog.Core.Models.Report;
using Blog.Core.SeedWorks;
using Blog.Core.SeedWorks.Constants;
using Blog.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers.Admin
{
    [Route("api/admin/royalty")]
    [ApiController]
    public class RoyaltyController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoyaltyService _royaltyService;
        public RoyaltyController(IUnitOfWork unitOfWork, IRoyaltyService royaltyService)
        {
            _unitOfWork = unitOfWork;
            _royaltyService = royaltyService;
        }

        [HttpGet]
        [Route("transaction")]
        [Authorize(Permissions.Royalty.GetTransactions)]
        public async Task<ActionResult<PagingResponse<TransactionResponse>>> GetTransactions(string? keyword,
            string fromDate, string toDate, int pageIndex = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(fromDate) || !DateTime.TryParse(fromDate, out DateTime convertFromDate))
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            if (string.IsNullOrEmpty(toDate) || !DateTime.TryParse(toDate, out DateTime convertToDate))
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            if (convertFromDate <= DateTime.MinValue && convertFromDate > convertToDate)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            if (pageIndex <= 0 || pageSize <= 0)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var result = await _unitOfWork.Transactions.GetTransactions(keyword, convertFromDate, convertToDate,
                pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet]
        [Route("royalty-report")]
        [Authorize(Permissions.Royalty.GetRoyaltyReport)]
        public async Task<ActionResult<PagingResponse<RoyaltyReportResponse>>> GetRoyaltyReport(
            string? username, string fromDate, string toDate, int pageIndex = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(fromDate) || !DateTime.TryParse(fromDate, out DateTime convertFromDate))
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            if (string.IsNullOrEmpty(toDate) || !DateTime.TryParse(toDate, out DateTime convertToDate))
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            if (convertFromDate <= DateTime.MinValue && convertFromDate > convertToDate)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            if (pageIndex <= 0 || pageSize <= 0)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var result = await _royaltyService.GetRoyaltyReportAsync(username, convertFromDate, convertToDate,
                pageIndex, pageSize);
            return Ok(result);
        }

        [HttpPost]
        [Route("{userId}")]
        [Authorize(Permissions.Royalty.Pay)]
        public async Task<IActionResult> PayRoyalty(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var fromUserId = User.GetUserId();
            await _royaltyService.PayRoyaltyForUserAsync(fromUserId, userId);
            return Ok();
        }
    }
}
