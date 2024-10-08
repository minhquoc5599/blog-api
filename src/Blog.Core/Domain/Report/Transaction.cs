﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Core.Domain.Report
{
    [Table("Transactions")]
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FromUserId { get; set; }

        [MaxLength(250)]
        [Required]
        public required string FromUserName { get; set; }
        public Guid ToUserId { get; set; }

        [MaxLength(250)]
        [Required]
        public required string ToUserName { get; set; }
        public double Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime DateCreated { get; set; }

        [MaxLength(250)]
        public string? Note { get; set; }
    }

    public enum TransactionType
    {
        RoyaltyPay
    }
}
