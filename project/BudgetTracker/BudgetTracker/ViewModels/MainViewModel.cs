using BudgetTracker.Models;
using BudgetTracker.Toast;
using BudgetTracker.Utils;
using BudgetTracker.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BudgetTracker.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<TransactionViewModel> Transactions { get; set; }

        private TransactionViewModel? _selectedTransaction;
        public TransactionViewModel? SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                if (_selectedTransaction != value)
                {
                    _selectedTransaction = value;
                    OnPropertyChanged(nameof(SelectedTransaction));

                    (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public decimal TotalIncome => Transactions.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount);
        public decimal TotalExpense => Transactions.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount);
        public decimal Balance => TotalIncome - TotalExpense;

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

        public MainViewModel()
        {
            Transactions = new ObservableCollection<TransactionViewModel>();
            AddCommand = new RelayCommand(AddTransaction);
            DeleteCommand = new RelayCommand(DeleteTransaction, () => SelectedTransaction != null);

        }

        private void AddTransaction()
        {
            //var newTransaction = new TransactionViewModel(new Transaction
            //{
            //    Date = DateTime.Now,
            //    Category = "New Category",
            //    Description = "New Description",
            //    Amount = 0,
            //    TransactionType = TransactionType.Expense // Default to Expense
            //});
            //Transactions.Add(newTransaction);
            //SelectedTransaction = newTransaction;
            Util.Log("show new transaction dialog");
            var dialog = new AddTransactionDialog()
            {
                DataContext = new AddTransactionViewModel()
            };

            var result = dialog.ShowDialog();
            Util.Log($"dialog result: {result}");

            if (result == false)
            {
                Util.Log("dialog result is false, return");
                ToastManager.Show("Cancel add transaction dialog!!!", ToastType.Error, position: ToastPosition.Center, durationMs: 2000);
                return;
            }

            Util.Log("checking dialog viewmodel");
            var viewModel = dialog.DataContext as AddTransactionViewModel;
            if (viewModel == null || viewModel.crearedTransaction == null)
                return;
            Util.Log("add new transaction");
            var newTransaction = new TransactionViewModel(viewModel.crearedTransaction);
            Transactions.Add(newTransaction);
            SelectedTransaction = newTransaction;

            updateSummary();

            ToastManager.Show("Add transaction successfully!!!", ToastType.Success, position: ToastPosition.Center, durationMs: 2000);

        }

        private void DeleteTransaction()
        {
            if (SelectedTransaction != null)
            {
                Transactions.Remove(SelectedTransaction);
                SelectedTransaction = null;
                updateSummary();
            }
        }

        private void updateSummary()
        {
            // update
            OnPropertyChanged(nameof(TotalIncome));
            OnPropertyChanged(nameof(TotalExpense));
            OnPropertyChanged(nameof(Balance));
        }
    }
}
