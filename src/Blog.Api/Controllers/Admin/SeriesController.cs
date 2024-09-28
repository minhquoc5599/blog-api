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
    [Route("api/admin/series")]
    [ApiController]
    public class SeriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SeriesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Permissions.Series.Create)]
        public async Task<IActionResult> CreateSeries([FromBody] CreateUpdateSeriesRequest request)
        {
            if (request == null)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var series = _mapper.Map<CreateUpdateSeriesRequest, Series>(request);
            _unitOfWork.Series.Add(series);

            var result = await _unitOfWork.CompleteAsync();
            return result > 0 ? Ok() : BadRequest();
        }

        [HttpPut("{id}")]
        [Authorize(Permissions.Series.Edit)]
        public async Task<IActionResult> UpdateSeries(Guid id, [FromBody] CreateUpdateSeriesRequest request)
        {
            if (request == null || id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var series = await _unitOfWork.Series.GetByIdAsync(id);
            if (series == null)
            {
                return NotFound(StatusMessage.NotFound.Series);
            }
            _mapper.Map(request, series);

            var result = await _unitOfWork.CompleteAsync();
            return result >= 0 ? Ok() : StatusCode(500, StatusMessage.InternalServerError);
        }

        [HttpDelete]
        [Authorize(Permissions.Series.Delete)]
        public async Task<IActionResult> DeleteSeries([FromQuery] Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            if (ids.Length == 1)
            {
                var series = await _unitOfWork.Series.GetByIdAsync(ids[0]);
                if (series == null)
                {
                    return NotFound(StatusMessage.NotFound.Series);
                }
                if (await _unitOfWork.Series.CheckExistPost(ids[0]))
                {
                    return Conflict($"The {series.Name} series contains posts and cannot be deleted");
                }
                _unitOfWork.Series.Remove(series);
            }
            else
            {
                foreach (var id in ids)
                {
                    var series = await _unitOfWork.Series.GetByIdAsync(id);
                    if (series == null)
                    {
                        continue;
                    }
                    if (await _unitOfWork.Series.CheckExistPost(id))
                    {
                        continue;
                    }
                    _unitOfWork.Series.Remove(series);
                }
            }

            var result = await _unitOfWork.CompleteAsync();
            return result > 0 ? Ok() : StatusCode(500, StatusMessage.InternalServerError);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize(Permissions.Series.View)]
        public async Task<ActionResult<SeriesDetailResponse>> GetSeriesById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var series = await _unitOfWork.Series.GetByIdAsync(id);
            if (series == null)
            {
                return NotFound(StatusMessage.NotFound.Role);
            }

            var result = _mapper.Map<SeriesDetailResponse>(series);
            return Ok(result);
        }

        [HttpGet]
        [Route("paging")]
        [Authorize(Permissions.Series.View)]
        public async Task<ActionResult<PagingResponse<SeriesResponse>>> GetSeriesPaging(string? keyword,
            int pageIndex, int pageSize = 10)
        {
            if (pageIndex <= 0 || pageSize <= 0)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var result = await _unitOfWork.Series.GetSeriesAsync(keyword, pageIndex, pageSize);

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Permissions.Series.View)]
        public async Task<ActionResult<List<SeriesResponse>>> GetAllSeries()
        {
            var result = await _unitOfWork.Series.GetAllAsync();
            var series = _mapper.Map<List<SeriesResponse>>(result);
            return Ok(series);
        }

        [Route("post-series")]
        [HttpPost()]
        [Authorize(Permissions.Series.AddPostInSeries)]
        public async Task<IActionResult> AddPostSeries([FromBody] AddPostSeriesRequest request)
        {
            if (request == null)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var isExisted = await _unitOfWork.Series.IsPostInSeries(request.SeriesId, request.PostId);
            if (isExisted)
            {
                return Conflict(StatusMessage.Conflict.PostInSeries);
            }
            await _unitOfWork.Series.AddPostToSeries(request.SeriesId, request.PostId, request.SortOrder);
            var result = await _unitOfWork.CompleteAsync();
            return result > 0 ? Ok() : BadRequest();
        }

        [Route("post-series")]
        [HttpDelete()]
        [Authorize(Permissions.Series.DeletePostInSeries)]
        public async Task<IActionResult> DeletePostSeries([FromBody] AddPostSeriesRequest request)
        {
            if (request == null)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var isExisted = await _unitOfWork.Series.IsPostInSeries(request.SeriesId, request.PostId);
            if (!isExisted)
            {
                return NotFound(StatusMessage.NotFound.PostInSeries);
            }
            await _unitOfWork.Series.RemovePostToSeries(request.SeriesId, request.PostId);
            var result = await _unitOfWork.CompleteAsync();
            return result > 0 ? Ok() : BadRequest();
        }

        [Route("post-series/{seriesId}")]
        [HttpGet()]
        [Authorize(Permissions.Series.GetPostsInSeries)]
        public async Task<ActionResult<List<PostResponse>>> GetPostsInSeries(Guid seriesId)
        {
            if (seriesId == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var posts = await _unitOfWork.Series.GetPostsInSeries(seriesId);
            return Ok(posts);
        }
    }
}
