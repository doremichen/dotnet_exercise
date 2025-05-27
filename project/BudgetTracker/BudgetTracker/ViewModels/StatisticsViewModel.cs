using BudgetTracker.Data.Repositories;
using BudgetTracker.Models;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTracker.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        public SeriesCollection PieSeries { get; set; } = null!;
        public SeriesCollection BarSeries { get; set; } = null!;

        public List<string> BarLabels { get; set; } = null!;
        public Func<double, string> Formatter { get; set; } = null!;

        // transacetion repository instance
        private readonly ITransactionRepository _transactionRepository = null!;

        // constructor
        public StatisticsViewModel()
        {
            _transactionRepository = new TransactionRepository();
            this.Formatter = value => value.ToString("C"); // Format as currency without decimal places
            // load data
            LoadData();
        }

        private async void LoadData()
        {
            // Get all transactions
            var transactions = (await _transactionRepository.GetAllAsync()).ToList();

            // Get category totals by transactiontype expense from transactions
            var categoryGroups = transactions
                .Where(t => t.TransactionType == "Expense")
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .ToList();

            // Create PieSeries for each category
            PieSeries = new SeriesCollection();
            foreach (var group in categoryGroups)
            {
                PieSeries.Add(new PieSeries
                {
                    Title = group.Category,
                    Values = new ChartValues<decimal> { group.Total },
                    DataLabels = true
                });
            }

            // Get monthly totals for bar chart
            var monthlyGroups = transactions
                .GroupBy(t => t.Date.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Month = g.Key,
                    Income = g.Where(t => t.TransactionType == TransactionType.Income.ToString()).Sum(t => t.Amount),
                    Expense = g.Where(t => t.TransactionType == TransactionType.Expense.ToString()).Sum(t => t.Amount)
                })
                .ToList();

            // Set BarLabels
            BarLabels = monthlyGroups.Select(g => g.Month).ToList();
            // Create BarSeries for income and expense
            BarSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Income",
                    Values = new ChartValues<decimal>(monthlyGroups.Select(g => g.Income)),
                    DataLabels = true
                },
                new ColumnSeries
                {
                    Title = "Expense",
                    Values = new ChartValues<decimal>(monthlyGroups.Select(g => g.Expense)),
                    DataLabels = true
                }
            };

            // Notify that the data has been loaded
            OnPropertyChanged(nameof(PieSeries));
            OnPropertyChanged(nameof(BarSeries));
            OnPropertyChanged(nameof(BarLabels));

        }
    }
}
