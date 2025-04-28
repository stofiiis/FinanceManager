using Npgsql;
using PersonalFinanceManager.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalFinanceManager
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = "Host=localhost;Port=5432;Database=finance_db;Username=postgres;Password=postgres";
        }

        public async Task InitializeDatabase()
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS transactions (
                        id SERIAL PRIMARY KEY,
                        amount DECIMAL NOT NULL,
                        type VARCHAR(10) NOT NULL,
                        category VARCHAR(50) NOT NULL,
                        date DATE NOT NULL,
                        note TEXT
                    )", conn);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Database initialization failed: {ex.Message}\n\nMake sure PostgreSQL is running in Docker.",
                    "Database Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public async Task<int> AddTransaction(Transaction transaction)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO transactions (amount, type, category, date, note)
                VALUES (@amount, @type, @category, @date, @note)
                RETURNING id", conn);

            cmd.Parameters.AddWithValue("amount", transaction.Amount);
            cmd.Parameters.AddWithValue("type", transaction.Type.ToString());
            cmd.Parameters.AddWithValue("category", transaction.Category);
            cmd.Parameters.AddWithValue("date", transaction.Date);
            cmd.Parameters.AddWithValue("note", transaction.Note ?? (object)DBNull.Value);

            return (int)await cmd.ExecuteScalarAsync();
        }

        public async Task<List<Transaction>> GetTransactions(DateTime? startDate = null, DateTime? endDate = null, string category = null)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = "SELECT id, amount, type, category, date, note FROM transactions WHERE 1=1";
            var parameters = new List<NpgsqlParameter>();

            if (startDate.HasValue)
            {
                query += " AND date >= @startDate";
                parameters.Add(new NpgsqlParameter("startDate", startDate.Value));
            }

            if (endDate.HasValue)
            {
                query += " AND date <= @endDate";
                parameters.Add(new NpgsqlParameter("endDate", endDate.Value));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query += " AND category = @category";
                parameters.Add(new NpgsqlParameter("category", category));
            }

            query += " ORDER BY date DESC";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddRange(parameters.ToArray());

            var transactions = new List<Transaction>();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                transactions.Add(new Transaction
                {
                    Id = reader.GetInt32(0),
                    Amount = reader.GetDecimal(1),
                    Type = Enum.Parse<TransactionType>(reader.GetString(2)),
                    Category = reader.GetString(3),
                    Date = reader.GetDateTime(4),
                    Note = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }

            return transactions;
        }

        public async Task<Dictionary<string, decimal>> GetMonthlyBalance(int year, int month)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            using var cmd = new NpgsqlCommand(@"
                SELECT 
                    SUM(CASE WHEN type = 'Income' THEN amount ELSE 0 END) as income,
                    SUM(CASE WHEN type = 'Expense' THEN amount ELSE 0 END) as expenses
                FROM transactions
                WHERE date BETWEEN @startDate AND @endDate", conn);

            cmd.Parameters.AddWithValue("startDate", startDate);
            cmd.Parameters.AddWithValue("endDate", endDate);

            using var reader = await cmd.ExecuteReaderAsync();
            var result = new Dictionary<string, decimal>
            {
                ["Income"] = 0,
                ["Expenses"] = 0,
                ["Balance"] = 0
            };

            if (await reader.ReadAsync())
            {
                result["Income"] = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                result["Expenses"] = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                result["Balance"] = result["Income"] - result["Expenses"];
            }

            return result;
        }

        public async Task<Dictionary<string, decimal>> GetCategoryBreakdown(DateTime startDate, DateTime endDate, TransactionType type)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                SELECT category, SUM(amount) as total
                FROM transactions
                WHERE date BETWEEN @startDate AND @endDate
                AND type = @type
                GROUP BY category
                ORDER BY total DESC", conn);

            cmd.Parameters.AddWithValue("startDate", startDate);
            cmd.Parameters.AddWithValue("endDate", endDate);
            cmd.Parameters.AddWithValue("type", type.ToString());

            var result = new Dictionary<string, decimal>();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result[reader.GetString(0)] = reader.GetDecimal(1);
            }

            return result;
        }

        public async Task<List<string>> GetCategories()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT DISTINCT category FROM transactions ORDER BY category", conn);

            var categories = new List<string>();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                categories.Add(reader.GetString(0));
            }

            return categories;
        }
    }
}
