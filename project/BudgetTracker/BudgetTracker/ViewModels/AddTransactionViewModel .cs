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
    public class AddTransactionViewModel : BaseViewModel
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public TransactionType SelectedTransactionType { get; set; } = TransactionType.Expense; // Default to Expense

        public ObservableCollection<string> Categories { get;} = new ObservableCollection<string>
        {
            "Food",
            "Transport",
            "Entertainment",
            "Utilities",
            "Rent",
            "Salary",
            "Other"
        };

        public ObservableCollection<string> TransactionTypes { get; } = new ObservableCollection<string>
        {
            "Income",
            "Expense"
        };

        // confirm command
        public ICommand ConfirmCommand { get; }

        public Transaction crearedTransaction { get; private set; } = null!;

        public AddTransactionViewModel()
        {
            ConfirmCommand = new RelayCommand(ConfirmTransaction);
        }

        private void ConfirmTransaction()
        {
            Util.Log("ConfirmTransaction called");
            if (!decimal.TryParse(Amount, out var amount))
            {
                // show invalid amount message
                MessageBox.Show("Invalid amount. Please enter a valid number.");
                return;
            }
            Util.Log($"Amount: {amount}");
            crearedTransaction = new Transaction
            {
                Date = Date,
                Category = Category,
                Description = Description,
                Amount = amount,
                TransactionType = SelectedTransactionType
            };
            Util.Log($"Transaction created: {crearedTransaction}");
            // Close the dialog or perform any other action needed after confirmation
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }
    }
}
