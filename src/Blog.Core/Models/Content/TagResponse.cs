using AutoMapper;
using Blog.Core.Domain.Content;

namespace Blog.Core.Models.Content
{
	public class TagResponse
    {
		public Guid Id { get; set; }
		public string Slug { get; set; }
		public required string Name { get; set; }
		public class AutoMapperProfiles : Profile
		{
			public AutoMapperProfiles()
			{
				CreateMap<Tag, TagResponse>();
			}
		}
	}
}
