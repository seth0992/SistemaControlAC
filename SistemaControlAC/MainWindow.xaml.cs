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
        private readonly ISessionService? _sessionService; // Allow nullability to avoid CS8601
        private readonly DispatcherTimer _activityTimer; // Marked as readonly and initialized in the constructor

        public MainWindow()
        {
            InitializeComponent();
            var app = (App)Application.Current;
            _authService = app.Services.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
            _sessionService = app.Services.GetService(typeof(ISessionService)) as ISessionService;

            // Initialize the _activityTimer to avoid CS8618
            _activityTimer = new DispatcherTimer();

            InitializeUserInterface();
            SetupActivityMonitoring();
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

        private void UpdateStatusBar()
        {
            StatusText.Text = $"Conectado • {DateTime.Now:dddd, dd MMMM yyyy HH:mm}";
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