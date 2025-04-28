using System.Windows;

namespace PersonalFinanceManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize database on startup
            var dbService = new DatabaseService();
            dbService.InitializeDatabase().Wait();
        }
    }
}
