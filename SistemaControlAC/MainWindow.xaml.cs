using SistemaControlAC.Core.Interfaces;
using SistemaControlAC.View;
using SistemaControlAC.ViewModel;
using System.Windows;
using System.Windows.Threading;

namespace SistemaControlAC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IAuthenticationService? _authService;
        private readonly ISessionService? _sessionService;
        private readonly DispatcherTimer _activityTimer;

        public MainWindow()
        {
            InitializeComponent();
            var app = (App)Application.Current;
            _authService = app.Services.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
            _sessionService = app.Services.GetService(typeof(ISessionService)) as ISessionService;

            _activityTimer = new DispatcherTimer();

            InitializeUserInterface();
            SetupActivityMonitoring();
            SetupMenuHandlers();
        }

        private void InitializeUserInterface()
        {
            // Mostrar información del usuario
            if (_sessionService != null && _sessionService.CurrentUser != null)
            {
                UserNameText.Text = _sessionService.GetUserFullName();
                UserRoleText.Text = $"({_sessionService.GetUserRole()})";

                // Ocultar secciones según el rol
                if (!_sessionService.IsAdmin())
                {
                    AdminSectionTitle.Visibility = Visibility.Collapsed;
                    UsersMenuButton.Visibility = Visibility.Collapsed;
                    ConfigMenuButton.Visibility = Visibility.Collapsed;
                }
            }

            // Establecer fecha y hora actual
            UpdateStatusBar();

            // Cargar vista de inicio
            ContentArea.Content = new Home();
        }

        private void SetupMenuHandlers()
        {
            // Configurar event handlers para los botones del menú
            HomeMenuButton.Click += (s, e) => NavigateToHome();
            ClientesMenuButton.Click += (s, e) => NavigateToClientes();
            EquiposMenuButton.Click += (s, e) => NavigateToEquipos();
            CitasMenuButton.Click += (s, e) => NavigateToCitas();
            OrdenesMenuButton.Click += (s, e) => NavigateToOrdenes();
            ReportesMenuButton.Click += (s, e) => NavigateToReportes();
            UsersMenuButton.Click += (s, e) => NavigateToUsuarios();
            ConfigMenuButton.Click += (s, e) => NavigateToConfiguracion();
        }

        #region Navegación

        private void NavigateToHome()
        {
            ContentArea.Content = new Home();
            UpdateStatusBar("Inicio");
        }

        private void NavigateToClientes()
        {
            var app = (App)Application.Current;
            var clienteService = app.Services.GetService(typeof(IClienteService)) as IClienteService;

            if (clienteService == null)
            {
                MessageBox.Show("Error: No se pudo cargar el servicio de clientes.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var clienteViewModel = new ClienteViewModel(clienteService, _sessionService!);
            var clienteView = new ClienteView { DataContext = clienteViewModel };
            ContentArea.Content = clienteView;
            UpdateStatusBar("Gestión de Clientes");
        }

        private void NavigateToEquipos()
        {
            // TODO: Implementar navegación a equipos
            MessageBox.Show("Funcionalidad de Equipos en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateStatusBar("Equipos");
        }

        private void NavigateToCitas()
        {
            // TODO: Implementar navegación a citas
            MessageBox.Show("Funcionalidad de Citas en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateStatusBar("Citas");
        }

        private void NavigateToOrdenes()
        {
            // TODO: Implementar navegación a órdenes
            MessageBox.Show("Funcionalidad de Órdenes de Trabajo en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateStatusBar("Órdenes de Trabajo");
        }

        private void NavigateToReportes()
        {
            // TODO: Implementar navegación a reportes
            MessageBox.Show("Funcionalidad de Reportes en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateStatusBar("Reportes");
        }

        private void NavigateToUsuarios()
        {
            if (_sessionService?.IsAdmin() != true)
            {
                MessageBox.Show("No tiene permisos para acceder a la gestión de usuarios.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Implementar navegación a usuarios
            MessageBox.Show("Funcionalidad de Usuarios en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateStatusBar("Gestión de Usuarios");
        }

        private void NavigateToConfiguracion()
        {
            if (_sessionService?.IsAdmin() != true)
            {
                MessageBox.Show("No tiene permisos para acceder a la configuración.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Implementar navegación a configuración
            MessageBox.Show("Funcionalidad de Configuración en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateStatusBar("Configuración");
        }

        #endregion

        private void SetupActivityMonitoring()
        {
            // Suscribirse al evento de sesión expirada
            if (_sessionService != null)
            {
                _sessionService.SessionExpired += OnSessionExpired;
            }

            // Timer para actualizar la actividad del usuario
            _activityTimer.Interval = TimeSpan.FromMinutes(1);
            _activityTimer.Tick += (s, e) =>
            {
                _sessionService?.UpdateActivity();
                UpdateStatusBar();
            };
            _activityTimer.Start();

            // Capturar eventos de actividad del usuario
            PreviewMouseMove += (s, e) => _sessionService?.UpdateActivity();
            PreviewKeyDown += (s, e) => _sessionService?.UpdateActivity();
        }

        private void OnSessionExpired(object? sender, EventArgs e)
        {
            _activityTimer.Stop();
            MessageBox.Show("Su sesión ha expirado. Por favor, inicie sesión nuevamente.",
                           "Sesión Expirada",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
            ShowLoginWindow();
        }

        private void UpdateStatusBar(string? section = null)
        {
            var status = $"Conectado • {DateTime.Now:dddd, dd MMMM yyyy HH:mm}";
            if (!string.IsNullOrEmpty(section))
            {
                status = $"{section} • {status}";
            }
            StatusText.Text = status;
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("¿Está seguro que desea cerrar sesión?",
                                       "Confirmar Cierre de Sesión",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _activityTimer?.Stop();

                if (_authService != null)
                {
                    await _authService.LogoutAsync();
                }

                // Limpiar SessionService
                _sessionService?.EndSession();

                ShowLoginWindow();
            }
        }

        private void ShowLoginWindow()
        {
            var loginWindow = new LoginWindow();
            var app = (App)Application.Current;
            loginWindow.DataContext = app.Services.GetService(typeof(LoginViewModel));
            loginWindow.Show();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _activityTimer?.Stop();
            if (_sessionService != null)
            {
                _sessionService.SessionExpired -= OnSessionExpired;
            }
            base.OnClosed(e);
        }
    }
}