using BudgetTracker.Data.Repositories;
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
        // get TransactionRepository
        private readonly TransactionRepository _transactionRepository = new TransactionRepository();

        public ObservableCollection<TransactionViewModel> Transactions { get; set; }



        private TransactionViewModel _selectedTransaction = null!;
        public TransactionViewModel SelectedTransaction
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

            // load transactions from database
            var allTransactions = _transactionRepository.GetAllAsync().Result;
            Transactions.Clear();

            foreach (var transaction in allTransactions)
            {
                Transactions.Add(new TransactionViewModel(transaction));
            }

            // set selected transaction to first transaction in list
            if (Transactions.Count > 0)
            {
                SelectedTransaction = Transactions[0];
            }
            else
            {
                SelectedTransaction = null!;
            }

        }

        private async void AddTransaction()
        {
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

            // add record in database
            await _transactionRepository.AddAsync(viewModel.crearedTransaction);

            ToastManager.Show("Add transaction successfully!!!", ToastType.Success, position: ToastPosition.Center, durationMs: 2000);

        }

        private void DeleteTransaction()
        {
            var transactionToDelete = SelectedTransaction.toModel();
            Transactions.Remove(SelectedTransaction);

            updateSummary();

            // delete record in database
            if (transactionToDelete != null)
            {
                _transactionRepository.DeleteAsync(transactionToDelete).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        // Handle the error
                        Util.Log($"Error deleting transaction: {t.Exception?.Message}");
                        ToastManager.Show("Error deleting transaction", ToastType.Error, position: ToastPosition.Center, durationMs: 2000);
                    }
                    else
                    {
                        ToastManager.Show("Delete transaction successfully!!!", ToastType.Success, position: ToastPosition.Center, durationMs: 2000);
                    }
                });
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
