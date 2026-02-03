using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantPOS.Infrastructure.Data;
using RestaurantPOS.ViewModels.Base;
using Serilog;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace RestaurantPOS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);

            Services = services.BuildServiceProvider();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs\\pos.log", rollingInterval: RollingInterval.Day)
    .       CreateLogger();


            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            Log.Information("POS Started");

        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Db
            var dbFolder = @"C:\RestaurantPOS";
            Directory.CreateDirectory(dbFolder);

            services.AddDbContext<PosDbContext>(options =>
                options.UseSqlite("Data Source=C:\\RestaurantPOS\\pos.db"));

            // Windows
            services.AddSingleton<MainWindow>();

            // ViewModels
            services.AddSingleton<MainViewModel>();

            // Services (empty for now)
            // services.AddScoped<IOrderService, OrderService>();
        }
    }

}
