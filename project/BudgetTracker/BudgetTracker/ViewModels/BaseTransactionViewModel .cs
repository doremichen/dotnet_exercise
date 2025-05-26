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
    public abstract class BaseTransactionViewModel : BaseViewModel
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

        public Transaction createdTransaction { get; private set; } = null!;

        public BaseTransactionViewModel()
        {
            ConfirmCommand = new RelayCommand(ConfirmTransaction);
        }

        private void ConfirmTransaction()
        {
            Util.Log("ConfirmTransaction called");

            createdTransaction = getTransaction();
            Util.Log($"Transaction: {createdTransaction}");
            // Close the dialog or perform any other action needed after confirmation
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        public abstract Transaction getTransaction();
        
    }
}
