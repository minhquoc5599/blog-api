using Blog.Core.Models.Base;
using Blog.Core.Models.Content;

namespace Blog.WebApp.Models.Series
{
    public class SeriesListViewModel
    {
        public PagingResponse<SeriesResponse> Series { get; set; }
    }
}
