using PersonalFinanceManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PersonalFinanceManager.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private decimal _currentMonthIncome;
        private decimal _currentMonthExpenses;
        private decimal _currentMonthBalance;
        private ObservableCollection<Transaction> _recentTransactions;
        private Dictionary<string, decimal> _topExpenseCategories;

        public DashboardViewModel()
        {
            _databaseService = new DatabaseService();
            LoadDashboardDataAsync();
        }

        public decimal CurrentMonthIncome
        {
            get => _currentMonthIncome;
            set
            {
                _currentMonthIncome = value;
                OnPropertyChanged();
            }
        }

        public decimal CurrentMonthExpenses
        {
            get => _currentMonthExpenses;
            set
            {
                _currentMonthExpenses = value;
                OnPropertyChanged();
            }
        }

        public decimal CurrentMonthBalance
        {
            get => _currentMonthBalance;
            set
            {
                _currentMonthBalance = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Transaction> RecentTransactions
        {
            get => _recentTransactions;
            set
            {
                _recentTransactions = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<string, decimal> TopExpenseCategories
        {
            get => _topExpenseCategories;
            set
            {
                _topExpenseCategories = value;
                OnPropertyChanged();
            }
        }

        private async void LoadDashboardDataAsync()
        {
            try
            {
                // Get current month balance
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var balanceData = await _databaseService.GetMonthlyBalance(currentYear, currentMonth);

                CurrentMonthIncome = balanceData["Income"];
                CurrentMonthExpenses = balanceData["Expenses"];
                CurrentMonthBalance = balanceData["Balance"];

                // Get recent transactions (last 10)
                var transactions = await _databaseService.GetTransactions(
                    DateTime.Now.AddMonths(-1),
                    DateTime.Now);

                RecentTransactions = new ObservableCollection<Transaction>(
                    transactions.Take(10));

                // Get top expense categories for current month
                var startDate = new DateTime(currentYear, currentMonth, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                TopExpenseCategories = await _databaseService.GetCategoryBreakdown(
                    startDate,
                    endDate,
                    TransactionType.Expense);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading dashboard data: {ex.Message}",
                    "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
