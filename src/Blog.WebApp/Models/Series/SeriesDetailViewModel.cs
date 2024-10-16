﻿using Blog.Core.Models.Base;
using Blog.Core.Models.Content;

namespace Blog.WebApp.Models.Series
{
    public class SeriesDetailViewModel
    {
        public SeriesResponse Series { get; set; }
        public PagingResponse<PostResponse> Posts { get; set; }
    }
}
