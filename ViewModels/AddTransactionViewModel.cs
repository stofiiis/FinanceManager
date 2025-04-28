using PersonalFinanceManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PersonalFinanceManager.ViewModels
{
    public class AddTransactionViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private decimal _amount;
        private TransactionType _transactionType = TransactionType.Expense;
        private string _category;
        private DateTime _date = DateTime.Today;
        private string _note;
        private List<string> _predefinedCategories;

        public AddTransactionViewModel()
        {
            _databaseService = new DatabaseService();
            SaveCommand = new RelayCommand(async _ => await SaveTransaction());

            // Default categories
            PredefinedCategories = new List<string>
            {
                "Food", "Rent", "Transportation", "Entertainment", "Utilities",
                "Healthcare", "Education", "Clothing", "Salary", "Investments", "Other"
            };
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }

        public TransactionType TransactionType
        {
            get => _transactionType;
            set
            {
                _transactionType = value;
                OnPropertyChanged();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        public string Note
        {
            get => _note;
            set
            {
                _note = value;
                OnPropertyChanged();
            }
        }

        public List<string> PredefinedCategories
        {
            get => _predefinedCategories;
            set
            {
                _predefinedCategories = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }

        private async Task SaveTransaction()
        {
            if (Amount <= 0)
            {
                MessageBox.Show("Amount must be greater than zero.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                MessageBox.Show("Category is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var transaction = new Transaction
            {
                Amount = Amount,
                Type = TransactionType,
                Category = Category,
                Date = Date,
                Note = Note
            };

            try
            {
                await _databaseService.AddTransaction(transaction);
                MessageBox.Show("Transaction saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reset form
                Amount = 0;
                TransactionType = TransactionType.Expense;
                Category = null;
                Date = DateTime.Today;
                Note = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
