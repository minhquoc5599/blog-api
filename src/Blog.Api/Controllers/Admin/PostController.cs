using AutoMapper;
using Blog.Api.Helps.Extensions;
using Blog.Core.Domain.Content;
using Blog.Core.Domain.Identity;
using Blog.Core.Models.Base;
using Blog.Core.Models.Content;
using Blog.Core.SeedWorks;
using Blog.Core.SeedWorks.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers.Admin
{
    [Route("api/admin/post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public PostController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize(Permissions.Posts.Create)]
        public async Task<ActionResult> CreatePost([FromBody] CreateUpdatePostRequest request)
        {
            if (await _unitOfWork.Posts.IsSlugAlreadyExisted(request.Slug))
            {
                return Conflict("Existed Slug");
            }
            var post = _mapper.Map<CreateUpdatePostRequest, Post>(request);
            var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(request.CategoryId);
            post.CategoryId = postCategory.Id;
            post.CategoryName = postCategory.Name;
            post.CategorySlug = postCategory.Slug;

            var userId = User.GetUserId();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            post.AuthorUserId = userId;
            post.AuthorName = user.GetFullName();
            post.AuthorUserName = user.UserName;
            _unitOfWork.Posts.Add(post);

            var result = await _unitOfWork.CompleteAsync();
            return result > 0 ? Created() : BadRequest();
        }

        [HttpPut("{id}")]
        [Authorize(Permissions.Posts.Edit)]
        public async Task<ActionResult> UpdatePost(Guid id, [FromBody] CreateUpdatePostRequest request)
        {
            if (await _unitOfWork.Posts.IsSlugAlreadyExisted(request.Slug))
            {
                return Conflict("Existed Slug");
            }

            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            if (post.CategoryId != request.CategoryId)
            {
                var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(request.CategoryId);
                post.CategoryId = postCategory.Id;
                post.CategoryName = postCategory.Name;
                post.CategorySlug = postCategory.Slug;
            }
            _mapper.Map(request, post);
            await _unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpDelete]
        [Authorize(Permissions.Posts.Delete)]
        public async Task<ActionResult> DeletePosts([FromQuery] Guid[] ids)
        {
            foreach (var id in ids)
            {
                var post = await _unitOfWork.Posts.GetByIdAsync(id);
                if (post == null)
                {
                    return NotFound();
                }
                _unitOfWork.Posts.Remove(post);
            }
            var result = await _unitOfWork.CompleteAsync();
            return result > 0 ? Ok() : BadRequest();
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize(Permissions.Posts.View)]
        public async Task<ActionResult<PostDetailResponse>> GetPostById(Guid id)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            var result = _mapper.Map<PostDetailResponse>(post);
            return Ok(result);
        }

        [HttpGet]
        [Route("paging")]
        [Authorize(Permissions.Posts.View)]
        public async Task<ActionResult<PagingResponse<PostResponse>>> GetPostsPaging(string? keyword, Guid? categoryId,
            int pageIndex, int pageSize = 10)
        {
            var userId = User.GetUserId();
            var result = await _unitOfWork.Posts.GetPostsAsync(keyword, userId, categoryId, pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("approve/{id}")]
        [Authorize(Permissions.Posts.Approve)]
        public async Task<IActionResult> ApprovePost(Guid id)
        {
            await _unitOfWork.Posts.Approve(id, User.GetUserId());
            await _unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpGet("approval-submit/{id}")]
        [Authorize(Permissions.Posts.SubmitForApproval)]
        public async Task<IActionResult> SendToApprove(Guid id)
        {
            await _unitOfWork.Posts.SubmitForApproval(id, User.GetUserId());
            await _unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpPost("reject/{id}")]
        [Authorize(Permissions.Posts.Reject)]
        public async Task<IActionResult> RejectPost(Guid id, [FromBody] ReturnBackRequest model)
        {
            await _unitOfWork.Posts.Reject(id, User.GetUserId(), model.Reason);
            await _unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpGet("reject-reason/{id}")]
        [Authorize(Permissions.Posts.RejectReason)]
        public async Task<ActionResult<string>> GetRejectReason(Guid id)
        {
            var note = await _unitOfWork.Posts.GetRejectReason(id);
            return Ok(note);
        }

        [HttpGet("post-activity-logs/{id}")]
        [Authorize(Permissions.Posts.GetPostActivityLogs)]
        public async Task<ActionResult<List<PostActivityLogResponse>>> GetPostActivityLogs(Guid id)
        {
            var logs = await _unitOfWork.Posts.GetPostActivityLogsWithPostId(id);
            return Ok(logs);
        }

        [HttpGet]
        [Route("series/{postId}")]
        [Authorize(Permissions.Posts.GetSeries)]
        public async Task<ActionResult<List<SeriesResponse>>> GetSeries(Guid postId)
        {
            var result = await _unitOfWork.Posts.GetSeriesWithPostId(postId);
            return Ok(result);
        }
    }
}
