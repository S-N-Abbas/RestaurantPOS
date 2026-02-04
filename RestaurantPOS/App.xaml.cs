using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantPOS.Infrastructure.Data;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Home;
using RestaurantPOS.ViewModels.Login;
using RestaurantPOS.ViewModels.Orders;
using RestaurantPOS.ViewModels.Shell;
using RestaurantPOS.ViewModels.Tables;
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


            var navigation = Services.GetRequiredService<INavigationService>();
            navigation.NavigateTo<LoginViewModel>();

            Log.Information("POS Started");

            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = Services.GetRequiredService<ShellViewModel>();
            mainWindow.Show();

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


            // Navigation
            services.AddSingleton<INavigationService, NavigationService>();

            // Shell
            services.AddSingleton<ShellViewModel>();

            // ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<TablesViewModel>();
            services.AddTransient<OrderViewModel>();
            services.AddTransient<TablesViewModel>();


            // Services (empty for now)
            // services.AddScoped<IOrderService, OrderService>();
        }
    }

}
