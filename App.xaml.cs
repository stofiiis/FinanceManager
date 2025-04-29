using System.Threading.Tasks;
using System.Windows;

namespace PersonalFinanceManager
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var dbService = new DatabaseService();
            await dbService.InitializeDatabase();
        }
    }
}
