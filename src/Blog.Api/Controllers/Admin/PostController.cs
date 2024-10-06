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
            if (request == null)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            if (await _unitOfWork.Posts.IsSlugAlreadyExisted(request.Slug))
            {
                return Conflict(StatusMessage.Conflict.Post);
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
            return result > 0 ? Created() : StatusCode(500, StatusMessage.InternalServerError);
        }

        [HttpPut("{id}")]
        [Authorize(Permissions.Posts.Edit)]
        public async Task<ActionResult> UpdatePost(Guid id, [FromBody] CreateUpdatePostRequest request)
        {
            if (request == null || id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound(StatusMessage.NotFound.Post);
            }

            if (!request.Slug.Equals(post.Slug))
            {
                if (await _unitOfWork.Posts.IsSlugAlreadyExisted(request.Slug))
                {
                    return Conflict(StatusMessage.Conflict.Post);
                }
            }

            if (post.CategoryId != request.CategoryId)
            {
                var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(request.CategoryId);
                post.CategoryId = postCategory.Id;
                post.CategoryName = postCategory.Name;
                post.CategorySlug = postCategory.Slug;
            }
            _mapper.Map(request, post);

            var result = await _unitOfWork.CompleteAsync();
            return result >= 0 ? Ok() : StatusCode(500, StatusMessage.InternalServerError);
        }

        [HttpDelete]
        [Authorize(Permissions.Posts.Delete)]
        public async Task<ActionResult> DeletePosts([FromQuery] Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            if (ids.Length == 1)
            {
                var post = await _unitOfWork.Posts.GetByIdAsync(ids[0]);
                if (post == null)
                {
                    return NotFound(StatusMessage.NotFound.Post);
                }
                _unitOfWork.Posts.Remove(post);
            }
            else
            {
                foreach (var id in ids)
                {
                    var post = await _unitOfWork.Posts.GetByIdAsync(id);
                    if (post == null)
                    {
                        continue;
                    }
                    _unitOfWork.Posts.Remove(post);
                }
            }

            var result = await _unitOfWork.CompleteAsync();
            return result > 0 ? Ok() : StatusCode(500, StatusMessage.InternalServerError);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize(Permissions.Posts.View)]
        public async Task<ActionResult<PostDetailResponse>> GetPostById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound(StatusMessage.NotFound.Post);
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
            if (pageIndex <= 0 || pageSize <= 0)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var userId = User.GetUserId();
            var result = await _unitOfWork.Posts.GetPostsAsync(keyword, userId, categoryId, pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("approve/{id}")]
        [Authorize(Permissions.Posts.Approve)]
        public async Task<IActionResult> ApprovePost(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            await _unitOfWork.Posts.Approve(id, User.GetUserId());
            await _unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpGet("approval-submit/{id}")]
        [Authorize(Permissions.Posts.SubmitForApproval)]
        public async Task<IActionResult> SendToApprove(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            await _unitOfWork.Posts.SubmitForApproval(id, User.GetUserId());
            await _unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpPost("reject/{id}")]
        [Authorize(Permissions.Posts.Reject)]
        public async Task<IActionResult> RejectPost(Guid id, [FromBody] ReturnBackRequest request)
        {
            if (request == null || id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            await _unitOfWork.Posts.Reject(id, User.GetUserId(), request.Reason);
            await _unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpGet("reject-reason/{id}")]
        [Authorize(Permissions.Posts.RejectReason)]
        public async Task<ActionResult<string>> GetRejectReason(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var note = await _unitOfWork.Posts.GetRejectReason(id);
            return Ok(note);
        }

        [HttpGet("post-activity-logs/{id}")]
        [Authorize(Permissions.Posts.GetPostActivityLogs)]
        public async Task<ActionResult<List<PostActivityLogResponse>>> GetPostActivityLogs(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var logs = await _unitOfWork.Posts.GetPostActivityLogsWithPostId(id);
            return Ok(logs);
        }

        [HttpGet]
        [Route("series/{postId}")]
        [Authorize(Permissions.Posts.GetSeries)]
        public async Task<ActionResult<List<SeriesResponse>>> GetSeries(Guid postId)
        {
            if (postId == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var result = await _unitOfWork.Posts.GetSeriesWithPostId(postId);
            return Ok(result);
        }
    }
}
