using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using SistemaControlAC.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SistemaControlAC.ViewModel
{
    public class ClienteFormViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly IClienteService _clienteService;
        private readonly ISessionService _sessionService;
        private readonly bool _isEditMode;
        private readonly Cliente? _originalCliente;

        private string _nombre = string.Empty;
        private string _apellido = string.Empty;
        private string _telefono = string.Empty;
        private string _telefonoSecundario = string.Empty;
        private string _email = string.Empty;
        private string _direccion = string.Empty;
        private string _ciudad = string.Empty;
        private string _codigoPostal = string.Empty;
        private string _rfc = string.Empty;
        private bool _recibeNotificacionesEmail = true;
        private bool _recibeNotificacionesWhatsApp = true;
        private bool _activo = true;

        public ClienteFormViewModel(IClienteService clienteService, ISessionService sessionService, Cliente? cliente = null)
        {
            _clienteService = clienteService;
            _sessionService = sessionService;
            _isEditMode = cliente != null;
            _originalCliente = cliente;

            Title = _isEditMode ? "Editar Cliente" : "Nuevo Cliente";

            // Cargar datos si es modo edición
            if (_isEditMode && cliente != null)
            {
                LoadClienteData(cliente);
            }

            // Inicializar comandos
            SaveCommand = new RelayCommand(async (param) => await SaveAsync(), (param) => CanSave());
            CancelCommand = new RelayCommand((param) => Cancel());
            ClearCommand = new RelayCommand((param) => Clear());
        }

        #region Propiedades

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string Apellido
        {
            get => _apellido;
            set => SetProperty(ref _apellido, value);
        }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string Telefono
        {
            get => _telefono;
            set => SetProperty(ref _telefono, value);
        }

        [StringLength(20, ErrorMessage = "El teléfono secundario no puede exceder 20 caracteres")]
        public string TelefonoSecundario
        {
            get => _telefonoSecundario;
            set => SetProperty(ref _telefonoSecundario, value);
        }

        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(255, ErrorMessage = "La dirección no puede exceder 255 caracteres")]
        public string Direccion
        {
            get => _direccion;
            set => SetProperty(ref _direccion, value);
        }

        [StringLength(50, ErrorMessage = "La ciudad no puede exceder 50 caracteres")]
        public string Ciudad
        {
            get => _ciudad;
            set => SetProperty(ref _ciudad, value);
        }

        [StringLength(10, ErrorMessage = "El código postal no puede exceder 10 caracteres")]
        public string CodigoPostal
        {
            get => _codigoPostal;
            set => SetProperty(ref _codigoPostal, value);
        }

        [StringLength(15, ErrorMessage = "El RFC no puede exceder 15 caracteres")]
        public string RFC
        {
            get => _rfc;
            set => SetProperty(ref _rfc, value);
        }

        public bool RecibeNotificacionesEmail
        {
            get => _recibeNotificacionesEmail;
            set => SetProperty(ref _recibeNotificacionesEmail, value);
        }

        public bool RecibeNotificacionesWhatsApp
        {
            get => _recibeNotificacionesWhatsApp;
            set => SetProperty(ref _recibeNotificacionesWhatsApp, value);
        }

        public bool Activo
        {
            get => _activo;
            set => SetProperty(ref _activo, value);
        }

        public bool IsEditMode => _isEditMode;

        #endregion

        #region Comandos

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ClearCommand { get; }

        #endregion

        #region IDataErrorInfo

        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                var context = new ValidationContext(this) { MemberName = columnName };
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateProperty(
                    GetType().GetProperty(columnName)?.GetValue(this),
                    context,
                    results);

                if (!isValid && results.Count > 0)
                {
                    return results[0].ErrorMessage ?? string.Empty;
                }

                // Validaciones adicionales
                switch (columnName)
                {
                    case nameof(Telefono):
                        if (!string.IsNullOrWhiteSpace(Telefono) && !IsValidPhoneNumber(Telefono))
                            return "El formato del teléfono no es válido";
                        break;

                    case nameof(TelefonoSecundario):
                        if (!string.IsNullOrWhiteSpace(TelefonoSecundario) && !IsValidPhoneNumber(TelefonoSecundario))
                            return "El formato del teléfono secundario no es válido";
                        break;

                    case nameof(RFC):
                        if (!string.IsNullOrWhiteSpace(RFC) && !IsValidRFC(RFC))
                            return "El formato del RFC no es válido";
                        break;
                }

                return string.Empty;
            }
        }

        #endregion

        #region Métodos

        private void LoadClienteData(Cliente cliente)
        {
            Nombre = cliente.Nombre;
            Apellido = cliente.Apellido;
            Telefono = cliente.Telefono;
            Email = cliente.Email;
            Direccion = cliente.Direccion;
            Ciudad = cliente.Ciudad ?? string.Empty;
            CodigoPostal = cliente.CodigoPostal ?? string.Empty;
            RecibeNotificacionesEmail = cliente.RecibeNotificacionesEmail;
            RecibeNotificacionesWhatsApp = cliente.RecibeNotificacionesWhatsApp;
            Activo = cliente.Activo;
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Nombre) &&
                   !string.IsNullOrWhiteSpace(Apellido) &&
                   !string.IsNullOrWhiteSpace(Telefono) &&
                   !string.IsNullOrWhiteSpace(Direccion) &&
                   !IsBusy;
        }

        private async Task SaveAsync()
        {
            try
            {
                if (!CanSave())
                {
                    MessageBox.Show("Por favor complete todos los campos requeridos.",
                                  "Validación",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                IsBusy = true;

                var cliente = CreateClienteFromForm();

                if (_isEditMode)
                {
                    var result = await _clienteService.UpdateAsync(cliente);
                    if (result.Success)
                    {
                        MessageBox.Show(result.Message,
                                      "Éxito",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        CloseWindow(true);
                    }
                    else
                    {
                        MessageBox.Show(result.Message,
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                }
                else
                {
                    var result = await _clienteService.CreateAsync(cliente);
                    if (result.Success)
                    {
                        MessageBox.Show(result.Message,
                                      "Éxito",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        CloseWindow(true);
                    }
                    else
                    {
                        MessageBox.Show(result.Message,
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Cancel()
        {
            var hasChanges = HasChanges();

            if (hasChanges)
            {
                var result = MessageBox.Show("Hay cambios sin guardar. ¿Está seguro que desea cancelar?",
                                           "Confirmar Cancelación",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;
            }

            CloseWindow(false);
        }

        private void Clear()
        {
            var result = MessageBox.Show("¿Está seguro que desea limpiar todos los campos?",
                                       "Confirmar Limpieza",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Nombre = string.Empty;
                Apellido = string.Empty;
                Telefono = string.Empty;
                TelefonoSecundario = string.Empty;
                Email = string.Empty;
                Direccion = string.Empty;
                Ciudad = string.Empty;
                CodigoPostal = string.Empty;
                RFC = string.Empty;
                RecibeNotificacionesEmail = true;
                RecibeNotificacionesWhatsApp = true;
                Activo = true;
            }
        }

        private Cliente CreateClienteFromForm()
        {
            var cliente = new Cliente
            {
                Nombre = Nombre.Trim(),
                Apellido = Apellido.Trim(),
                Telefono = Telefono.Trim(),
                Email = string.IsNullOrWhiteSpace(Email) ? string.Empty : Email.Trim(),
                Direccion = Direccion.Trim(),
                Ciudad = string.IsNullOrWhiteSpace(Ciudad) ? null : Ciudad.Trim(),
                CodigoPostal = string.IsNullOrWhiteSpace(CodigoPostal) ? null : CodigoPostal.Trim(),
                RecibeNotificacionesEmail = RecibeNotificacionesEmail,
                RecibeNotificacionesWhatsApp = RecibeNotificacionesWhatsApp,
                Activo = Activo
            };

            if (_isEditMode && _originalCliente != null)
            {
                cliente.Id = _originalCliente.Id;
                cliente.FechaRegistro = _originalCliente.FechaRegistro;
            }

            return cliente;
        }

        private bool HasChanges()
        {
            if (!_isEditMode || _originalCliente == null)
                return !string.IsNullOrWhiteSpace(Nombre) ||
                       !string.IsNullOrWhiteSpace(Apellido) ||
                       !string.IsNullOrWhiteSpace(Telefono) ||
                       !string.IsNullOrWhiteSpace(TelefonoSecundario) ||
                       !string.IsNullOrWhiteSpace(Email) ||
                       !string.IsNullOrWhiteSpace(Direccion) ||
                       !string.IsNullOrWhiteSpace(Ciudad) ||
                       !string.IsNullOrWhiteSpace(CodigoPostal) ||
                       !string.IsNullOrWhiteSpace(RFC);

            return Nombre != _originalCliente.Nombre ||
                   Apellido != _originalCliente.Apellido ||
                   Telefono != _originalCliente.Telefono ||
                   Email != _originalCliente.Email ||
                   Direccion != _originalCliente.Direccion ||
                   Ciudad != (_originalCliente.Ciudad ?? string.Empty) ||
                   CodigoPostal != (_originalCliente.CodigoPostal ?? string.Empty) ||
                   RecibeNotificacionesEmail != _originalCliente.RecibeNotificacionesEmail ||
                   RecibeNotificacionesWhatsApp != _originalCliente.RecibeNotificacionesWhatsApp ||
                   Activo != _originalCliente.Activo;
        }

        private void CloseWindow(bool dialogResult)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = dialogResult;
                    window.Close();
                    break;
                }
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Contar dígitos
            var digitCount = phoneNumber.Count(char.IsDigit);
            return digitCount >= 8 && digitCount <= 15;
        }

        private bool IsValidRFC(string rfc)
        {
            if (string.IsNullOrWhiteSpace(rfc))
                return true; // RFC es opcional

            // Validación básica de RFC mexicano
            rfc = rfc.Trim().ToUpper();

            // Persona física: 4 letras + 6 dígitos + 3 caracteres
            // Persona moral: 3 letras + 6 dígitos + 3 caracteres
            return rfc.Length == 13 || rfc.Length == 12;
        }

        #endregion
    }
}
