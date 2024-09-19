using AutoMapper;
using Blog.Core.Domain.Content;
using System.ComponentModel.DataAnnotations;

namespace Blog.Core.Models.Content
{
    public class SeriesDetailResponse : SeriesResponse
    {
        [MaxLength(250)]
        public string? SeoDescription { get; set; }

        [MaxLength(250)]
        public string? Thumbnail { set; get; }

        public string? Content { get; set; }

        public class AutoMapperProfiles : Profile
        {
            public AutoMapperProfiles()
            {
                CreateMap<Series, SeriesDetailResponse>();
            }
        }
    }
}
