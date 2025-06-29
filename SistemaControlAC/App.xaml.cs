using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaControlAC.Core.Entities;
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
        internal ServiceProvider _serviceProvider = null!;
        private IConfiguration _configuration = null!;

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
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IEquipoRepository, EquipoRepository>();

            // Servicios de Dominio
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<ISessionService, SessionService>();
            services.AddSingleton<ICredentialService, CredentialService>(); // Nuevo servicio
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IEquipoService, EquipoService>();


            // ViewModels
            services.AddTransient<LoginViewModel>(provider => new LoginViewModel(
                provider.GetRequiredService<IAuthenticationService>(),
                provider.GetRequiredService<ISessionService>(),
                provider.GetRequiredService<ICredentialService>()));

            services.AddTransient<HomeViewModel>();
            services.AddTransient<ClienteViewModel>();
            services.AddTransient<ClienteFormViewModel>();
            services.AddTransient<ClienteDetailViewModel>();


            services.AddTransient<EquipoViewModel>();
            services.AddTransient<EquipoFormViewModel>();
            services.AddTransient<EquipoDetailViewModel>();

     
            // Ventanas y Views
            services.AddTransient<LoginWindow>(provider =>
            {
                var window = new LoginWindow();
                window.DataContext = provider.GetRequiredService<LoginViewModel>();
                return window;
            });

            services.AddTransient<MainWindow>();
            services.AddTransient<ClienteView>();
            services.AddTransient<ClienteFormWindow>();
            services.AddTransient<ClienteDetailWindow>();
            services.AddTransient<EquipoView>();
            services.AddTransient<EquipoFormWindow>();
            services.AddTransient<EquipoDetailWindow>();

            // Factory para ViewModels que requieren parámetros
            services.AddTransient<Func<ClienteViewModel>>(provider =>
                () => new ClienteViewModel(
                    provider.GetRequiredService<IClienteService>(),
                    provider.GetRequiredService<ISessionService>()));

            services.AddTransient<Func<Cliente?, ClienteFormViewModel>>(provider =>
                (cliente) => new ClienteFormViewModel(
                    provider.GetRequiredService<IClienteService>(),
                    provider.GetRequiredService<ISessionService>(),
                    cliente));

            services.AddTransient<Func<Cliente, ClienteDetailViewModel>>(provider =>
                (cliente) => new ClienteDetailViewModel(
                    provider.GetRequiredService<IClienteService>(),
                    provider.GetRequiredService<ISessionService>(),
                    cliente));

            services.AddTransient<Func<EquipoViewModel>>(provider =>
    () => new EquipoViewModel(
        provider.GetRequiredService<IEquipoService>(),
        provider.GetRequiredService<ISessionService>()));

            services.AddTransient<Func<EquipoAireAcondicionado?, EquipoFormViewModel>>(provider =>
                (equipo) => new EquipoFormViewModel(
                    provider.GetRequiredService<IEquipoService>(),
                    provider.GetRequiredService<ISessionService>(),
                    equipo));

            services.AddTransient<Func<EquipoAireAcondicionado, EquipoDetailViewModel>>(provider =>
                (equipo) => new EquipoDetailViewModel(
                    provider.GetRequiredService<IEquipoService>(),
                    provider.GetRequiredService<ISessionService>(),
                    equipo));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}