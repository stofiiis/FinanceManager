using PersonalFinanceManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PersonalFinanceManager.ViewModels
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private int _selectedYear;
        private int _selectedMonth;
        private decimal _income;
        private decimal _expenses;
        private decimal _balance;
        private Dictionary<string, decimal> _categoryBreakdown;
        private DateTime _startDate;
        private DateTime _endDate;

        public ReportsViewModel()
        {
            _databaseService = new DatabaseService();

            // Initialize with current month/year
            SelectedYear = DateTime.Now.Year;
            SelectedMonth = DateTime.Now.Month;

            // Set date range for category breakdown
            StartDate = new DateTime(SelectedYear, SelectedMonth, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);

            GenerateReportCommand = new RelayCommand(async _ => await GenerateReport());

            // Load initial report
            GenerateReport().ConfigureAwait(false);
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                _selectedYear = value;
                OnPropertyChanged();
            }
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                _selectedMonth = value;
                OnPropertyChanged();
            }
        }

        public decimal Income
        {
            get => _income;
            set
            {
                _income = value;
                OnPropertyChanged();
            }
        }

        public decimal Expenses
        {
            get => _expenses;
            set
            {
                _expenses = value;
                OnPropertyChanged();
            }
        }

        public decimal Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<string, decimal> CategoryBreakdown
        {
            get => _categoryBreakdown;
            set
            {
                _categoryBreakdown = value;
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

        public List<int> Years => Enumerable.Range(DateTime.Now.Year - 5, 6).ToList();

        public List<int> Months => Enumerable.Range(1, 12).ToList();

        public ICommand GenerateReportCommand { get; }

        private async Task GenerateReport()
        {
            try
            {
                // Update date range
                StartDate = new DateTime(SelectedYear, SelectedMonth, 1);
                EndDate = StartDate.AddMonths(1).AddDays(-1);

                // Get monthly balance
                var balanceData = await _databaseService.GetMonthlyBalance(SelectedYear, SelectedMonth);
                Income = balanceData["Income"];
                Expenses = balanceData["Expenses"];
                Balance = balanceData["Balance"];

                // Get category breakdown for expenses
                CategoryBreakdown = await _databaseService.GetCategoryBreakdown(StartDate, EndDate, TransactionType.Expense);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error generating report: {ex.Message}",
                    "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public double GetPercentage(decimal value)
        {
            if (Expenses <= 0)
                return 0;

            return (double)(value / Expenses * 100);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
