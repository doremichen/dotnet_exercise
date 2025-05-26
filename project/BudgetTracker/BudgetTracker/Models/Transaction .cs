using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTracker.Models
{
    public class Transaction
    {
        // The Id property is the primary key for the Transaction entity.
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }

        // <summary>
        // upate the current transaction with values from another transaction
        public void UpdateFrom(Transaction other)
        {
            if (other == null) return;

            Date = other.Date;
            Category = other.Category;
            Description = other.Description;
            Amount = other.Amount;
            TransactionType = other.TransactionType;
        }
    }
}
