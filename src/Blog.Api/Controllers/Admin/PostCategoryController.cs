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
            var postCategory = _mapper.Map<CreateUpdatePostCategoryRequest, PostCategory>(request);
            _unitOfWork.PostCategories.Add(postCategory);

            var result = await _unitOfWork.CompleteAsync();
            return result > 0 ? Ok() : BadRequest();
        }

        [HttpPut("{id}")]
        [Authorize(Permissions.PostCategories.Edit)]
        public async Task<IActionResult> UpdatePostCategory(Guid id, [FromBody] CreateUpdatePostCategoryRequest request)
        {
            var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(id);
            if (postCategory == null)
            {
                return NotFound();
            }

            _mapper.Map(request, postCategory);

            var result = await _unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpDelete]
        [Authorize(Permissions.PostCategories.Delete)]
        public async Task<IActionResult> DeletePostCategory([FromQuery] Guid[] ids)
        {
            foreach (var id in ids)
            {
                var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(id);
                if (postCategory == null)
                {
                    return NotFound();
                }
                if (await _unitOfWork.PostCategories.CheckExistPost(id))
                {
                    return Ok("The category contains posts and cannot be deleted");
                }
                _unitOfWork.PostCategories.Remove(postCategory);
            }
            var result = await _unitOfWork.CompleteAsync();
            return result > 0 ? Ok() : BadRequest();
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize(Permissions.PostCategories.View)]
        public async Task<ActionResult<PostCategoryResponse>> GetPostCategoryById(Guid id)
        {
            var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(id);
            if (postCategory == null)
            {
                return NotFound();
            }
            var result = _mapper.Map<PostCategoryResponse>(postCategory);
            return Ok(result);
        }

        [HttpGet]
        [Route("paging")]
        [Authorize(Permissions.PostCategories.View)]
        public async Task<ActionResult<PagingResponse<PostCategoryResponse>>> GetPostCategoriesPaging(
            string? keyword, int pageIndex, int pageSize = 10)
        {
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
