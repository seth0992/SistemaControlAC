using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using SistemaControlAC.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SistemaControlAC.ViewModel
{
    public class EquipoFormViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly IEquipoService _equipoService;
        private readonly IClienteService _clienteService;
        private readonly ISessionService _sessionService;
        private readonly bool _isEditMode;
        private readonly EquipoAireAcondicionado? _originalEquipo;

        private int _clienteId;
        private string _marca = string.Empty;
        private string _modelo = string.Empty;
        private string _numeroSerie = string.Empty;
        private string _tipo = string.Empty;
        private string _capacidad = string.Empty;
        private string _ubicacion = string.Empty;
        private DateTime? _fechaInstalacion;
        private bool _activo = true;

        private ObservableCollection<Cliente> _clientes;
        private List<string> _tiposEquipo;
        private List<string> _marcas;

        public EquipoFormViewModel(IEquipoService equipoService, ISessionService sessionService, EquipoAireAcondicionado? equipo = null)
        {
            _equipoService = equipoService;
            _sessionService = sessionService;
            _isEditMode = equipo != null;
            _originalEquipo = equipo;

            // Obtener servicio de clientes del contenedor de DI
            var app = (App)Application.Current;
            _clienteService = app.Services.GetService(typeof(IClienteService)) as IClienteService ??
                             throw new InvalidOperationException("IClienteService not found");

            Title = _isEditMode ? "Editar Equipo" : "Nuevo Equipo";

            _clientes = new ObservableCollection<Cliente>();
            _tiposEquipo = new List<string>();
            _marcas = new List<string>();

            // Cargar datos iniciales
            _ = LoadInitialDataAsync();

            // Cargar datos si es modo edición
            if (_isEditMode && equipo != null)
            {
                LoadEquipoData(equipo);
            }

            // Inicializar comandos
            SaveCommand = new RelayCommand(async (param) => await SaveAsync(), (param) => CanSave());
            CancelCommand = new RelayCommand((param) => Cancel());
            ClearCommand = new RelayCommand((param) => Clear());
        }

        #region Propiedades

        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        public int ClienteId
        {
            get => _clienteId;
            set => SetProperty(ref _clienteId, value);
        }

        [Required(ErrorMessage = "La marca es requerida")]
        [StringLength(50, ErrorMessage = "La marca no puede exceder 50 caracteres")]
        public string Marca
        {
            get => _marca;
            set => SetProperty(ref _marca, value);
        }

        [Required(ErrorMessage = "El modelo es requerido")]
        [StringLength(50, ErrorMessage = "El modelo no puede exceder 50 caracteres")]
        public string Modelo
        {
            get => _modelo;
            set => SetProperty(ref _modelo, value);
        }

        [StringLength(100, ErrorMessage = "El número de serie no puede exceder 100 caracteres")]
        public string NumeroSerie
        {
            get => _numeroSerie;
            set => SetProperty(ref _numeroSerie, value);
        }

        [Required(ErrorMessage = "El tipo de equipo es requerido")]
        public string Tipo
        {
            get => _tipo;
            set => SetProperty(ref _tipo, value);
        }

        [StringLength(20, ErrorMessage = "La capacidad no puede exceder 20 caracteres")]
        public string Capacidad
        {
            get => _capacidad;
            set => SetProperty(ref _capacidad, value);
        }

        [Required(ErrorMessage = "La ubicación es requerida")]
        [StringLength(100, ErrorMessage = "La ubicación no puede exceder 100 caracteres")]
        public string Ubicacion
        {
            get => _ubicacion;
            set => SetProperty(ref _ubicacion, value);
        }

        public DateTime? FechaInstalacion
        {
            get => _fechaInstalacion;
            set => SetProperty(ref _fechaInstalacion, value);
        }

        public bool Activo
        {
            get => _activo;
            set => SetProperty(ref _activo, value);
        }

        // Colecciones para ComboBoxes
        public ObservableCollection<Cliente> Clientes
        {
            get => _clientes;
            set => SetProperty(ref _clientes, value);
        }

        public List<string> TiposEquipo
        {
            get => _tiposEquipo;
            set => SetProperty(ref _tiposEquipo, value);
        }

        public List<string> Marcas
        {
            get => _marcas;
            set => SetProperty(ref _marcas, value);
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
                    case nameof(FechaInstalacion):
                        if (FechaInstalacion.HasValue && FechaInstalacion > DateTime.Today)
                            return "La fecha de instalación no puede ser futura";
                        break;
                }

                return string.Empty;
            }
        }

        #endregion

        #region Métodos

        private async Task LoadInitialDataAsync()
        {
            try
            {
                IsBusy = true;

                // Cargar clientes
                var clientes = await _clienteService.GetActiveAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Clientes.Clear();
                    foreach (var cliente in clientes.OrderBy(c => c.Nombre))
                    {
                        Clientes.Add(cliente);
                    }
                });

                // Cargar tipos de equipo
                TiposEquipo = await _equipoService.GetTiposEquipoAsync();

                // Cargar marcas
                Marcas = await _equipoService.GetMarcasAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos iniciales: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LoadEquipoData(EquipoAireAcondicionado equipo)
        {
            ClienteId = equipo.ClienteId;
            Marca = equipo.Marca;
            Modelo = equipo.Modelo;
            NumeroSerie = equipo.NumeroSerie ?? string.Empty;
            Tipo = equipo.Tipo;
            Capacidad = equipo.Capacidad ?? string.Empty;
            Ubicacion = equipo.Ubicacion;
            FechaInstalacion = equipo.FechaInstalacion;
            Activo = equipo.Activo;
        }

        private bool CanSave()
        {
            return ClienteId > 0 &&
                   !string.IsNullOrWhiteSpace(Marca) &&
                   !string.IsNullOrWhiteSpace(Modelo) &&
                   !string.IsNullOrWhiteSpace(Tipo) &&
                   !string.IsNullOrWhiteSpace(Ubicacion) &&
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

                var equipo = CreateEquipoFromForm();

                if (_isEditMode)
                {
                    var result = await _equipoService.UpdateAsync(equipo);
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
                    var result = await _equipoService.CreateAsync(equipo);
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
                ClienteId = 0;
                Marca = string.Empty;
                Modelo = string.Empty;
                NumeroSerie = string.Empty;
                Tipo = string.Empty;
                Capacidad = string.Empty;
                Ubicacion = string.Empty;
                FechaInstalacion = null;
                Activo = true;
            }
        }

        private EquipoAireAcondicionado CreateEquipoFromForm()
        {
            var equipo = new EquipoAireAcondicionado
            {
                ClienteId = ClienteId,
                Marca = Marca.Trim(),
                Modelo = Modelo.Trim(),
                NumeroSerie = string.IsNullOrWhiteSpace(NumeroSerie) ? null : NumeroSerie.Trim(),
                Tipo = Tipo.Trim(),
                Capacidad = string.IsNullOrWhiteSpace(Capacidad) ? null : Capacidad.Trim(),
                Ubicacion = Ubicacion.Trim(),
                FechaInstalacion = FechaInstalacion,
                Activo = Activo
            };

            if (_isEditMode && _originalEquipo != null)
            {
                equipo.Id = _originalEquipo.Id;
            }

            return equipo;
        }

        private bool HasChanges()
        {
            if (!_isEditMode || _originalEquipo == null)
                return ClienteId > 0 ||
                       !string.IsNullOrWhiteSpace(Marca) ||
                       !string.IsNullOrWhiteSpace(Modelo) ||
                       !string.IsNullOrWhiteSpace(NumeroSerie) ||
                       !string.IsNullOrWhiteSpace(Tipo) ||
                       !string.IsNullOrWhiteSpace(Capacidad) ||
                       !string.IsNullOrWhiteSpace(Ubicacion);

            return ClienteId != _originalEquipo.ClienteId ||
                   Marca != _originalEquipo.Marca ||
                   Modelo != _originalEquipo.Modelo ||
                   NumeroSerie != (_originalEquipo.NumeroSerie ?? string.Empty) ||
                   Tipo != _originalEquipo.Tipo ||
                   Capacidad != (_originalEquipo.Capacidad ?? string.Empty) ||
                   Ubicacion != _originalEquipo.Ubicacion ||
                   FechaInstalacion != _originalEquipo.FechaInstalacion ||
                   Activo != _originalEquipo.Activo;
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

        #endregion
    }
}