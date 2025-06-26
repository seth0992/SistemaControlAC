using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaControlAC.Core.Interfaces;
using SistemaControlAC.Data.Context;
using SistemaControlAC.Data.Repositories;
using SistemaControlAC.Services;
using SistemaControlAC.View;
using SistemaControlAC.ViewModel;
using System.IO;
using System.Windows;

namespace SistemaControlAC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal ServiceProvider _serviceProvider;
        private IConfiguration _configuration;

        public IServiceProvider Services => _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configurar servicios
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            // Iniciar con la ventana de login
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configuración
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(_configuration);

            // Entity Framework
            services.AddDbContext<ApplicationDbContext>(options =>
                 options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")));

            // Repositorios
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();

            // Servicios
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<ISessionService, SessionService>();

            // ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<HomeViewModel>();

            // Ventanas
            services.AddTransient<LoginWindow>(provider =>
            {
                var window = new LoginWindow();
                window.DataContext = provider.GetRequiredService<LoginViewModel>();
                return window;
            });

            services.AddTransient<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }

}
