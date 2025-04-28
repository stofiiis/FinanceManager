using PersonalFinanceManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PersonalFinanceManager.ViewModels
{
    public class TransactionViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<Transaction> _transactions;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _selectedCategory;
        private List<string> _categories;

        public TransactionViewModel()
        {
            _databaseService = new DatabaseService();

            // Default date range to current month
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);

            LoadTransactionsCommand = new RelayCommand(async _ => await LoadTransactions());
            LoadCategoriesAsync();
        }

        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set
            {
                _transactions = value;
                OnPropertyChanged();
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        public List<string> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadTransactionsCommand { get; }

        private async Task LoadTransactions()
        {
            var transactions = await _databaseService.GetTransactions(
                StartDate,
                EndDate,
                string.IsNullOrEmpty(SelectedCategory) ? null : SelectedCategory);

            Transactions = new ObservableCollection<Transaction>(transactions);
        }

        private async void LoadCategoriesAsync()
        {
            Categories = await _databaseService.GetCategories();
            Categories = new List<string> { "" }.Concat(Categories).ToList();
            SelectedCategory = "";

            // Load transactions after categories are loaded
            await LoadTransactions();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Simple relay command implementation
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
