using BudgetTracker.Models;
using BudgetTracker.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTracker.ViewModels
{
    public class TransactionViewModel : BaseViewModel
    {
        private Transaction _transaction;

        public TransactionViewModel(Transaction transaction)
        {
            this._transaction = transaction;
        }

        public Guid id => _transaction.Id;

        public DateTime Date
        {
            get => _transaction.Date;
            set
            {
                if (_transaction.Date != value)
                {
                    _transaction.Date = value;
                    OnPropertyChanged(nameof(Date));
                }
            }
        }

        public string Category
        {
            get => _transaction.Category;
            set
            {
                if (_transaction.Category != value)
                {
                    _transaction.Category = value;
                    OnPropertyChanged(nameof(Category));
                }
            }
        }

        public string Description
        {
            get => _transaction.Description;
            set
            {
                if (_transaction.Description != value)
                {
                    _transaction.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal Amount
        {
            get => _transaction.Amount;
            set
            {
                if (_transaction.Amount != value)
                {
                    _transaction.Amount = value;
                    OnPropertyChanged();
                }
            }
        }

        public TransactionType TransactionType
        {
            get => _transaction.TransactionType;
            set
            {
                if (_transaction.TransactionType != value)
                {
                    _transaction.TransactionType = value;
                    OnPropertyChanged();
                }
            }
        }

        public Transaction toModel()
        {
            return _transaction;
        }

        // update transaction
        public void UpdateFrom(Transaction updated)
        {
            Date = updated.Date;
            Category = updated.Category;
            Description = updated.Description;
            Amount = updated.Amount;
            TransactionType = updated.TransactionType;

            // update UI
            OnPropertyChanged(nameof(Date));
            OnPropertyChanged(nameof(Category));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Amount));
            OnPropertyChanged(nameof(TransactionType));
        }
    }
}
