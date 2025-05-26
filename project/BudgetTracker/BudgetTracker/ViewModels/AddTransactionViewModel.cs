using BudgetTracker.Models;
using BudgetTracker.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BudgetTracker.ViewModels
{
    public class AddTransactionViewModel : BaseTransactionViewModel
    {
        public override Transaction getTransaction()
        {
            if (!decimal.TryParse(Amount, out var amount))
            {
                // show invalid amount message
                MessageBox.Show("Invalid amount. Please enter a valid number.");
                return null!;
            }
            Util.Log($"Amount: {amount}");

            return new Transaction
            {
                Date = Date,
                Category = Category,
                Description = Description,
                Amount = amount,
                TransactionType = SelectedTransactionType
            };
        }
    }
}
