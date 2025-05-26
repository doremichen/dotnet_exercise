using BudgetTracker.Models;
using BudgetTracker.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BudgetTracker.ViewModels
{
    public class EditTransactionViewModel : BaseTransactionViewModel
    {
        // need update transaction
        private Transaction _transaction;


        // Constructor with Transaction parameters
        public EditTransactionViewModel(Transaction transaction)
        {
            // Initialize properties or perform any setup needed for editing a transaction
            Date = transaction.Date;
            Category = transaction.Category;
            Description = transaction.Description;
            Amount = transaction.Amount.ToString();
            SelectedTransactionType = transaction.TransactionType;

            _transaction = transaction;
        }


        public override Transaction getTransaction()
        {
            if (!decimal.TryParse(Amount, out var amount))
            {
                // show invalid amount message
                MessageBox.Show("Invalid amount. Please enter a valid number.");
                return null!;
            }
            Util.Log($"Amount: {amount}");

            // Update the existing transaction with new values
            _transaction.Date = Date;
            _transaction.Category = Category;
            _transaction.Description = Description;
            _transaction.Amount = amount;
            _transaction.TransactionType = SelectedTransactionType;

            return _transaction;
        }
    }
}
