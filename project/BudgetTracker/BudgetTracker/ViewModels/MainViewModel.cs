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
using System.Windows;
using System.Windows.Input;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        public decimal TotalIncome => Transactions.Where(t => t.TransactionType == TransactionType.Income.ToString()).Sum(t => t.Amount);
        public decimal TotalExpense => Transactions.Where(t => t.TransactionType == TransactionType.Expense.ToString()).Sum(t => t.Amount);
        public decimal Balance => TotalIncome - TotalExpense;

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }

        public MainViewModel()
        {
            Transactions = new ObservableCollection<TransactionViewModel>();
            AddCommand = new RelayCommand(AddTransaction);
            DeleteCommand = new RelayCommand(DeleteTransaction, () => SelectedTransaction != null);
            EditCommand = new RelayCommand(EditTransaction, () => SelectedTransaction != null);

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

        private async void EditTransaction()
        {
            // check selected transaction is null or not
            if (SelectedTransaction == null)
            {
                ToastManager.Show("Please select a transaction to edit", ToastType.Error, position: ToastPosition.Center, durationMs: 2000);
                return;
            }
            Util.Log("show edit transaction dialog");
            var dialog = new AddTransactionDialog()
            {
                Title = "Edit Transaction",
                DataContext = new EditTransactionViewModel(SelectedTransaction.toModel())
            };

            var result = dialog.ShowDialog();
            Util.Log($"dialog result: {result}");
            if (result == false)
            {
                Util.Log("dialog result is false, return");
                ToastManager.Show("Cancel edit transaction dialog!!!", ToastType.Error, position: ToastPosition.Center, durationMs: 2000);
                return;
            }
            bool flowControl = await updateUIAndDatabase(dialog);
            if (!flowControl)
            {
                return;
            }

            // show toast
            ToastManager.Show("Edit transaction successfully!!!", ToastType.Success, position: ToastPosition.Center, durationMs: 2000);


        }

        private async Task<bool> updateUIAndDatabase(AddTransactionDialog dialog)
        {
            var viewModel = dialog.DataContext as EditTransactionViewModel;
            if (viewModel == null || viewModel.createdTransaction == null)
            {
                ToastManager.Show("Edit transaction failed!!!", ToastType.Error, position: ToastPosition.Center, durationMs: 2000);
                return false;
            }
            Transaction updatedTransaction = viewModel.createdTransaction;

            updateUI(updatedTransaction);

            (bool flowControl, bool value) = await updateDb(updatedTransaction);
            if (!flowControl)
            {
                return value;
            }
            return true;
        }

        private async Task<(bool flowControl, bool value)> updateDb(Transaction updatedTransaction)
        {
            Util.Log("update the transaction record!!!");
            // update selected transaction
            var tranctionInDb = _transactionRepository.GetByIdAsync(updatedTransaction.Id).Result;
            if (tranctionInDb == null)
            {
                ToastManager.Show("Transaction not found in database", ToastType.Error, position: ToastPosition.Center, durationMs: 2000);
                return (flowControl: false, value: false);
            }
            tranctionInDb.UpdateFrom(updatedTransaction);
            await _transactionRepository.UpdateAsync(updatedTransaction);
            return (flowControl: true, value: default);
        }

        private void updateUI(Transaction updatedTransaction)
        {
            // update the transaction in view model
            Application.Current.Dispatcher.Invoke(() =>
            {
                // update the transaction in view model
                Util.Log("update the transaction in view model");
                var transaction = this.Transactions.FirstOrDefault(t => t.toModel().Id == updatedTransaction.Id);
                if (transaction == null)
                {
                    ToastManager.Show("Transaction not found in transactions list", ToastType.Error, position: ToastPosition.Center, durationMs: 2000);
                    return;
                }
                transaction.UpdateFrom(updatedTransaction);

                updateSummary();
            });
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
            if (viewModel == null || viewModel.createdTransaction == null)
                return;
            Util.Log("add new transaction");
            var newTransaction = new TransactionViewModel(viewModel.createdTransaction);
            Transactions.Add(newTransaction);
            SelectedTransaction = newTransaction;

            updateSummary();

            // add record in database
            await _transactionRepository.AddAsync(viewModel.createdTransaction);

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
