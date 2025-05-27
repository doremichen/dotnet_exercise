using BudgetTracker.Data.Repositories;
using BudgetTracker.Models;
using BudgetTracker.Toast;
using BudgetTracker.Utils;
using BudgetTracker.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
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

        public ICommand ExportCommand { get; }

        public ICommand ImportCommand { get; }

        public ICommand ViewStaticsDataCommand { get; }

        public MainViewModel()
        {
            Transactions = new ObservableCollection<TransactionViewModel>();
            AddCommand = new RelayCommand(AddTransaction);
            DeleteCommand = new RelayCommand(DeleteTransaction, () => SelectedTransaction != null);
            EditCommand = new RelayCommand(EditTransaction, () => SelectedTransaction != null);
            ExportCommand = new RelayCommand(ExportToCsv);
            // Update the ImportCommand initialization to use an async void method instead of Task
            ImportCommand = new RelayCommand(async () => await ImportFromCsv());
            ViewStaticsDataCommand = new RelayCommand(ViewStaticsData());
            LoadDataSet();

        }

        private static Action ViewStaticsData()
        {
            return () =>
            {
                Util.Log("show statistics view");
                var statisticsView = new StatisticsView();
                statisticsView.Show();
            };
        }

        private void LoadDataSet()
        {
            // load transactions from database
            var allTransactions = _transactionRepository.GetAllAsync().Result;
            RefreshTransactionList(allTransactions);

            // set selected transaction to first transaction in list
            SelectedTransaction = (Transactions.Count > 0) ? Transactions[0] : null!;

        }

        private void RefreshTransactionList(IEnumerable<Transaction> allTransactions)
        {
            Transactions.Clear();

            foreach (var transaction in allTransactions)
            {
                Transactions.Add(new TransactionViewModel(transaction));
            }
        }

        private async Task ImportFromCsv()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv",
                // FileName = {years}.csv
                FileName = $"{DateTime.Now.Year}_transactions.csv"
            };

            // show dialog
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var lines = File.ReadAllLines(openFileDialog.FileName, Encoding.UTF8);

                    if (lines.Length <= 1)
                    {
                        ToastManager.Show("No data in CSV file", ToastType.Error, position: ToastPosition.Center, durationMs: 2000);
                        return;
                    }

                    var preview = new List<Transaction>();

                    for (int i = 1; i < lines.Length; i++)
                    {
                        var fields = ParseCsvLine(lines[i]);

                        if (fields.Length < 6)
                            continue;

                        if (!DateTime.TryParse(fields[1], out var date) ||
                            !decimal.TryParse(fields[4], out var amount))
                        {
                            ToastManager.Show("Line {i + 1} format error!!!", ToastType.Error, position: ToastPosition.Center, durationMs: 2000);
                            return;
                        }

                        preview.Add(new Transaction
                        {
                            Id = Guid.TryParse(fields[0], out var id) ? id : Guid.Empty,
                            Date = date,
                            Category = fields[2],
                            Description = fields[3],
                            Amount = amount,
                            TransactionType = fields[5]
                        });
                    }

                    // show summary information
                    var result = MessageBox.Show(
                        $"Will import {preview.Count} datas，to be continnue？",
                        "preImport", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Clear data in database before import
                        await _transactionRepository.ClearAllAsync();

                        // Clear the existing transactions
                        Transactions.Clear();

                        foreach (var item in preview)
                        {
                            TransactionViewModel itemVm = new(item);
                            // add to database
                            await _transactionRepository.AddAsync(itemVm.toModel());
                            Transactions.Add(itemVm);
                        }

                        ToastManager.Show("Success import CSV file！", ToastType.Success, ToastPosition.Center, 2000);
                    }
                }
                catch (Exception ex)
                {
                    ToastManager.Show("Import fail：" + ex.Message, ToastType.Error, position: ToastPosition.Center, durationMs: 2000);
                   
                }
            }

        }
        private string[] ParseCsvLine(string line)
        {
            var pattern = @"(?:^|,)(?:""(?<val>(?:[^""]|"""")*)""|(?<val>[^"",]*))";
            var matches = Regex.Matches(line, pattern);
            return matches.Cast<Match>()
                          .Select(m => m.Groups["val"].Value.Replace("\"\"", "\""))
                          .ToArray();
        }

        private void ExportToCsv()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv",
                // FileName = {years_month_day}.csv
                FileName = $"{DateTime.Now:yyyy_MM_dd}_transactions.csv"
            };

            // show dialog
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // write transactions to csv file
                    using (var writer = new System.IO.StreamWriter(saveFileDialog.FileName, false, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
                    {
                        // write header
                        writer.WriteLine("Id,Date,Category,Description,Amount,TransactionType");
                        // write each transaction
                        foreach (var transaction in Transactions)
                        {
                            writer.WriteLine($"{transaction.id},{Escape(transaction.Date.ToString("yyyy-MM-dd"))},{Escape(transaction.Category)},{Escape(transaction.Description)},{transaction.Amount},{transaction.TransactionType}");
                        }
                    }
                    ToastManager.Show("Export transactions to CSV successfully!!!", ToastType.Success, position: ToastPosition.Center, durationMs: 2000);

                    // auto open the file after export
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveFileDialog.FileName,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Util.Log($"Error exporting transactions to CSV: {ex.Message}");
                    ToastManager.Show("Error exporting transactions to CSV", ToastType.Error, position: ToastPosition.Center, durationMs: 2000);
                }
            }
        }

        private string Escape(string input)
        {
            if (input.Contains(",") || input.Contains("\""))
            {
                return $"\"{input.Replace("\"", "\"\"")}\"";
            }
            return input;
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

        private async void DeleteTransaction()
        {
            var transactionToDelete = SelectedTransaction.toModel();
            Transactions.Remove(SelectedTransaction);

            updateSummary();

            // delete record in database
            if (transactionToDelete != null)
            {
                // delete record in database
                if (transactionToDelete != null)
                {
                    try
                    {
                        await _transactionRepository.DeleteAsync(transactionToDelete);
                        ToastManager.Show("Delete transaction successfully!!!", ToastType.Success, position: ToastPosition.Center, durationMs: 2000);
                    }
                    catch (Exception ex)
                    {
                        Util.Log($"Error deleting transaction: {ex.Message}");
                        ToastManager.Show("Error deleting transaction", ToastType.Error, position: ToastPosition.Center, durationMs: 2000);
                    }
                }
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
