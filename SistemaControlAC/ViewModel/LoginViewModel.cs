using SistemaControlAC.Core.Interfaces;
using SistemaControlAC.Utilities;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SistemaControlAC.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ISessionService _sessionService;
        private readonly ICredentialService _credentialService;
      

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _rememberMe = false;
        private bool _isLoadingCredentials = false;

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
            set
            {
                if (SetProperty(ref _rememberMe, value))
                {
                    // Si se desmarca RememberMe, limpiar credenciales guardadas
                    if (!value && !_isLoadingCredentials)
                    {
                        _ = _credentialService.ClearCredentialsAsync();
                    }
                }
            }
        }

        public bool IsLoadingCredentials
        {
            get => _isLoadingCredentials;
            set => SetProperty(ref _isLoadingCredentials, value);
        }

        // Comandos
        public ICommand LoginCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ClearCredentialsCommand { get; }

        public LoginViewModel(IAuthenticationService authService, ISessionService sessionService, ICredentialService credentialService)
        {
            _authService = authService;
            _sessionService = sessionService;
            _credentialService = credentialService;
            Title = "Iniciar Sesión - Sistema Control AC";

            // Inicializar comandos
            LoginCommand = new RelayCommand(async (param) => await LoginAsync());
            CancelCommand = new RelayCommand((param) => Cancel());
            ClearCredentialsCommand = new RelayCommand(async (param) => await ClearCredentialsAsync());

            // Cargar credenciales guardadas al inicializar
            _ = LoadSavedCredentialsAsync();
        }
        private async Task ClearCredentialsAsync()
        {
            try
            {
                await _credentialService.ClearCredentialsAsync();
                Username = string.Empty;
                Password = string.Empty;
                RememberMe = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al limpiar credenciales: {ex.Message}";
            }
        }
        private async Task LoadSavedCredentialsAsync()
        {
            try
            {
                IsLoadingCredentials = true;

                var savedCredentials = await _credentialService.GetSavedCredentialsAsync();
                if (savedCredentials.HasValue)
                {
                    Username = savedCredentials.Value.Username;
                    Password = savedCredentials.Value.Password;
                    RememberMe = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar credenciales: {ex.Message}");
                // No mostrar error al usuario, simplemente no cargar credenciales
            }
            finally
            {
                IsLoadingCredentials = false;
            }
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
                    // Guardar credenciales si RememberMe está marcado
                    if (RememberMe)
                    {
                        try
                        {
                            await _credentialService.SaveCredentialsAsync(Username, Password);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error al guardar credenciales: {ex.Message}");
                            // No interrumpir el login por error al guardar credenciales
                        }
                    }
                    else
                    {
                        // Limpiar credenciales si RememberMe no está marcado
                        try
                        {
                            await _credentialService.ClearCredentialsAsync();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error al limpiar credenciales: {ex.Message}");
                        }
                    }

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
                // Limpiar contraseña por seguridad solo si no se está recordando
                if (!RememberMe)
                {
                    Password = string.Empty;
                }
            }
        }

        private void Cancel()
        {
            Application.Current.Shutdown();
        }
    }
}