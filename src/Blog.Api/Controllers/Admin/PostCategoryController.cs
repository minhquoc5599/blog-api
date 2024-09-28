using AutoMapper;
using Blog.Core.Domain.Content;
using Blog.Core.Models.Base;
using Blog.Core.Models.Content;
using Blog.Core.SeedWorks;
using Blog.Core.SeedWorks.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers.Admin
{
    [Route("api/admin/post-category")]
    [ApiController]
    public class PostCategoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PostCategoryController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Permissions.PostCategories.Create)]
        public async Task<IActionResult> CreatePostCategory([FromBody] CreateUpdatePostCategoryRequest request)
        {
            if (request == null)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var postCategory = _mapper.Map<CreateUpdatePostCategoryRequest, PostCategory>(request);
            _unitOfWork.PostCategories.Add(postCategory);

            var result = await _unitOfWork.CompleteAsync();
            return result > 0 ? Ok() : StatusCode(500, StatusMessage.InternalServerError);
        }

        [HttpPut("{id}")]
        [Authorize(Permissions.PostCategories.Edit)]
        public async Task<IActionResult> UpdatePostCategory(Guid id, [FromBody] CreateUpdatePostCategoryRequest request)
        {
            if (request == null || id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(id);
            if (postCategory == null)
            {
                return NotFound(StatusMessage.NotFound.PostCategory);
            }

            _mapper.Map(request, postCategory);

            var result = await _unitOfWork.CompleteAsync();
            return result >= 0 ? Ok() : StatusCode(500, StatusMessage.InternalServerError);
        }

        [HttpDelete]
        [Authorize(Permissions.PostCategories.Delete)]
        public async Task<IActionResult> DeletePostCategory([FromQuery] Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            if (ids.Length == 1)
            {
                var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(ids[0]);
                if (postCategory == null)
                {
                    return NotFound(StatusMessage.NotFound.PostCategory);
                }
                if (await _unitOfWork.PostCategories.CheckExistPost(ids[0]))
                {
                    return Conflict($"The {postCategory.Name} post category contains posts and cannot be deleted");
                }
                _unitOfWork.PostCategories.Remove(postCategory);
            }
            else
            {
                foreach (var id in ids)
                {
                    var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(id);
                    if (postCategory == null)
                    {
                        continue;
                    }
                    if (await _unitOfWork.PostCategories.CheckExistPost(id))
                    {
                        continue;
                    }
                    _unitOfWork.PostCategories.Remove(postCategory);
                }
            }

            var result = await _unitOfWork.CompleteAsync();
            return result > 0 ? Ok() : StatusCode(500, StatusMessage.InternalServerError);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize(Permissions.PostCategories.View)]
        public async Task<ActionResult<PostCategoryResponse>> GetPostCategoryById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(id);
            if (postCategory == null)
            {
                return NotFound(StatusMessage.NotFound.PostCategory);
            }

            var result = _mapper.Map<PostCategoryResponse>(postCategory);
            return Ok(result);
        }

        [HttpGet]
        [Route("paging")]
        [Authorize(Permissions.PostCategories.View)]
        public async Task<ActionResult<PagingResponse<PostCategoryResponse>>> GetPostCategoriesPaging(
            string? keyword, int pageIndex = 1, int pageSize = 10)
        {
            if (pageIndex <= 0 || pageSize <= 0)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var result = await _unitOfWork.PostCategories.GetPostCategoriesAsync(keyword, pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Permissions.PostCategories.View)]
        public async Task<ActionResult<List<PostCategoryResponse>>> GetPostCategories()
        {
            var query = await _unitOfWork.PostCategories.GetAllAsync();
            var model = _mapper.Map<List<PostCategoryResponse>>(query);
            return Ok(model);
        }
    }
}
