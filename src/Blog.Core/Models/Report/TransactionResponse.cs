using AutoMapper;
using Blog.Core.Domain.Report;

namespace Blog.Core.Models.Report
{
    public class TransactionResponse
    {
        public Guid Id { get; set; }
        public Guid FromUserId { get; set; }
        public required string FromUserName { get; set; }
        public Guid ToUserId { get; set; }
        public required string ToUserName { get; set; }
        public double Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime DateCreated { get; set; }
        public string? Note { get; set; }
        public class AutoMapperProfiles : Profile
        {
            public AutoMapperProfiles()
            {
                CreateMap<Transaction, TransactionResponse>();
            }
        }
    }
}
