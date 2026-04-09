using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantPOS.Infrastructure.Data;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Cover;
using RestaurantPOS.ViewModels.Home;
using RestaurantPOS.ViewModels.Login;
using RestaurantPOS.ViewModels.Orders;
using RestaurantPOS.ViewModels.Payments;
using RestaurantPOS.ViewModels.Shell;
using RestaurantPOS.ViewModels.Tables;
using Serilog;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using RestaurantPOS.ViewModels.BackOffice.Users;
using RestaurantPOS.ViewModels.BackOffice.Settings;
using RestaurantPOS.ViewModels.BackOffice;

namespace RestaurantPOS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override async void OnStartup(StartupEventArgs e)
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

            var orderStore = Services.GetRequiredService<OrderStore>();

            await orderStore.InitializeAsync();

            mainWindow.Show();

        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Db
            var dbFolder = @"C:\RestaurantPOS";
            Directory.CreateDirectory(dbFolder);

            services.AddDbContext<PosDbContext>(options =>
                options.UseSqlite("Data Source=C:\\RestaurantPOS\\pos.db"));

            services.AddDbContextFactory<PosDbContext>(options =>
            {
                options.UseSqlite("Data Source=C:\\RestaurantPOS\\pos.db");
            });

            // Windows
            services.AddSingleton<MainWindow>();


            // Navigation
            services.AddSingleton<INavigationService, NavigationService>();

            // Other Services
            services.AddScoped<IMenuDataService, MenuDataService>();
            services.AddSingleton<IOrderContextService, OrderContextService>();
            services.AddScoped<ITableService, TableService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddScoped<OrderService>();
            services.AddSingleton<OrderStore>();
            services.AddSingleton<TableStore>();
            services.AddSingleton<UserSessionService>();
            services.AddSingleton<AuthorizationService>();
            services.AddSingleton<SettingsService>();
            services.AddScoped<IMenuAdminService, MenuAdminService>();


            // Shell
            services.AddSingleton<ShellViewModel>();

            // ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<TablesViewModel>();
            services.AddTransient<OrderViewModel>();
            services.AddTransient<PaymentViewModel>();
            services.AddTransient<CoverSelectorViewModel>();
            services.AddTransient<UsersViewModel>();
            services.AddTransient<BackOfficeViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<OrderSwitcherViewModel>();
        }
    }

}
