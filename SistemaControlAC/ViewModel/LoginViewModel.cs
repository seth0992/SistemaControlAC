using SistemaControlAC.Core.Interfaces;
using SistemaControlAC.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SistemaControlAC.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ISessionService _sessionService;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _rememberMe;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        // Comandos
        public ICommand LoginCommand { get; }
        public ICommand CancelCommand { get; }

        public LoginViewModel(IAuthenticationService authService, ISessionService sessionService)
        {
            _authService = authService;
            _sessionService = sessionService;
            Title = "Iniciar Sesión - Sistema Control AC";

            // Inicializar comandos
            LoginCommand = new RelayCommand(async (param) => await LoginAsync());
            CancelCommand = new RelayCommand((param) => Cancel());
        }

        private async Task LoginAsync()
        {
            try
            {
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(Username))
                {
                    ErrorMessage = "Por favor ingrese su nombre de usuario";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Por favor ingrese su contraseña";
                    return;
                }

                IsBusy = true;

                var result = await _authService.LoginAsync(Username, Password);

                if (result.Success)
                {
                    // Actualizar SessionService
                    if (result.Usuario != null)
                    {
                        _sessionService.CurrentUser = result.Usuario;
                    }

                    // Navegar a la ventana principal
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var mainWindow = new MainWindow();
                        mainWindow.Show();

                        // Cerrar la ventana de login
                        foreach (Window window in Application.Current.Windows)
                        {
                            if (window.DataContext == this)
                            {
                                window.Close();
                                break;
                            }
                        }
                    });
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al iniciar sesión: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                Password = string.Empty; // Limpiar contraseña por seguridad
            }
        }

        private void Cancel()
        {
            Application.Current.Shutdown();
        }
    }
}
