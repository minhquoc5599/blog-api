﻿using AutoMapper;
using Blog.Core.Domain.Content;
using System.ComponentModel.DataAnnotations;

namespace Blog.Core.Models.Content
{
    public class PostResponse
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
        public string? Thumbnail { get; set; }
        public int ViewCount { get; set; }
        public DateTime DateCreated { get; set; }

        public required string CategorySlug { get; set; }
        public required string CategoryName { get; set; }
        public string AuthorUserName { get; set; }
        public string AuthorName { get; set; }

        public class AutoMapperProfiles : Profile
        {
            public AutoMapperProfiles()
            {
                CreateMap<Post, PostResponse>();
            }
        }
    }
}
