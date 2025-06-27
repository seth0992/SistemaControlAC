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
    public class CitaFormViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly ICitaService _citaService;
        private readonly IClienteService _clienteService;
        private readonly ISessionService _sessionService;
        private readonly bool _isEditMode;
        private readonly Cita? _originalCita;

        private int _clienteId;
        private int? _equipoId;
        private int? _tecnicoAsignadoId;
        private DateTime _fechaProgramada = DateTime.Today.AddDays(1);
        private TimeSpan _horaInicio = TimeSpan.FromHours(9);
        private TimeSpan? _horaFin = TimeSpan.FromHours(11);
        private string _tipoServicio = string.Empty;
        private string _estado = "Programada";
        private string _descripcion = string.Empty;
        private decimal? _costoEstimado;

        private ObservableCollection<Cliente> _clientes;
        private ObservableCollection<EquipoAireAcondicionado> _equiposCliente;
        private ObservableCollection<Usuario> _tecnicos;
        private List<string> _tiposServicio;

        public CitaFormViewModel(ICitaService citaService, ISessionService sessionService, Cita? cita = null)
        {
            _citaService = citaService;
            _sessionService = sessionService;
            _isEditMode = cita != null;
            _originalCita = cita;

            // Obtener servicios del contenedor de DI
            var app = (App)Application.Current;
            _clienteService = app.Services.GetService(typeof(IClienteService)) as IClienteService ??
                             throw new InvalidOperationException("IClienteService not found");

            Title = _isEditMode ? "Editar Cita" : "Nueva Cita";

            _clientes = new ObservableCollection<Cliente>();
            _equiposCliente = new ObservableCollection<EquipoAireAcondicionado>();
            _tecnicos = new ObservableCollection<Usuario>();
            _tiposServicio = new List<string>();

            // Cargar datos iniciales
            _ = LoadInitialDataAsync();

            // Cargar datos si es modo edición
            if (_isEditMode && cita != null)
            {
                LoadCitaData(cita);
            }

            // Inicializar comandos
            SaveCommand = new RelayCommand(async (param) => await SaveAsync(), (param) => CanSave());
            CancelCommand = new RelayCommand((param) => Cancel());
            ClearCommand = new RelayCommand((param) => Clear());
            ClienteChangedCommand = new RelayCommand(async (param) => await OnClienteChangedAsync());
            CalcularDuracionCommand = new RelayCommand((param) => CalcularDuracion());
        }

        #region Propiedades

        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        public int ClienteId
        {
            get => _clienteId;
            set => SetProperty(ref _clienteId, value);
        }

        public int? EquipoId
        {
            get => _equipoId;
            set => SetProperty(ref _equipoId, value);
        }

        public int? TecnicoAsignadoId
        {
            get => _tecnicoAsignadoId;
            set => SetProperty(ref _tecnicoAsignadoId, value);
        }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime FechaProgramada
        {
            get => _fechaProgramada;
            set => SetProperty(ref _fechaProgramada, value);
        }

        [Required(ErrorMessage = "La hora de inicio es requerida")]
        public TimeSpan HoraInicio
        {
            get => _horaInicio;
            set => SetProperty(ref _horaInicio, value);
        }

        public TimeSpan? HoraFin
        {
            get => _horaFin;
            set => SetProperty(ref _horaFin, value);
        }

        [Required(ErrorMessage = "Debe seleccionar un tipo de servicio")]
        public string TipoServicio
        {
            get => _tipoServicio;
            set => SetProperty(ref _tipoServicio, value);
        }

        public string Estado
        {
            get => _estado;
            set => SetProperty(ref _estado, value);
        }

        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        public decimal? CostoEstimado
        {
            get => _costoEstimado;
            set => SetProperty(ref _costoEstimado, value);
        }

        // Colecciones para ComboBoxes
        public ObservableCollection<Cliente> Clientes
        {
            get => _clientes;
            set => SetProperty(ref _clientes, value);
        }

        public ObservableCollection<EquipoAireAcondicionado> EquiposCliente
        {
            get => _equiposCliente;
            set => SetProperty(ref _equiposCliente, value);
        }

        public ObservableCollection<Usuario> Tecnicos
        {
            get => _tecnicos;
            set => SetProperty(ref _tecnicos, value);
        }

        public List<string> TiposServicio
        {
            get => _tiposServicio;
            set => SetProperty(ref _tiposServicio, value);
        }

        public List<string> Estados => new List<string>
        {
            "Programada",
            "En Proceso",
            "Completada",
            "Cancelada"
        };

        public bool IsEditMode => _isEditMode;

        // Propiedades computadas
        public Cliente? ClienteSeleccionado => Clientes.FirstOrDefault(c => c.Id == ClienteId);
        public Usuario? TecnicoSeleccionado => Tecnicos.FirstOrDefault(t => t.Id == TecnicoAsignadoId);

        public string DuracionEstimada
        {
            get
            {
                if (HoraFin.HasValue && HoraFin > HoraInicio)
                {
                    var duracion = HoraFin.Value - HoraInicio;
                    return $"{duracion.Hours}h {duracion.Minutes}min";
                }
                return "No definida";
            }
        }

        #endregion

        #region Comandos

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ClienteChangedCommand { get; }
        public ICommand CalcularDuracionCommand { get; }

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
                    case nameof(FechaProgramada):
                        if (FechaProgramada.Date < DateTime.Today)
                            return "La fecha no puede ser en el pasado";
                        break;

                    case nameof(HoraInicio):
                        if (HoraInicio < TimeSpan.FromHours(8) || HoraInicio > TimeSpan.FromHours(18))
                            return "Horario debe estar entre 8:00 AM y 6:00 PM";
                        break;

                    case nameof(HoraFin):
                        if (HoraFin.HasValue && HoraFin <= HoraInicio)
                            return "La hora de fin debe ser mayor que la hora de inicio";
                        break;

                    case nameof(CostoEstimado):
                        if (CostoEstimado.HasValue && CostoEstimado < 0)
                            return "El costo no puede ser negativo";
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

                // Cargar tipos de servicio
                TiposServicio = await _citaService.GetTiposServicioAsync();

                // Cargar técnicos
                await LoadTecnicosAsync();
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

        private async Task LoadTecnicosAsync()
        {
            try
            {
                // Simular carga de técnicos - esto debería venir del servicio de usuarios
                var app = (App)Application.Current;
                var usuarioRepository = app.Services.GetService(typeof(IUsuarioRepository)) as IUsuarioRepository;

                if (usuarioRepository != null)
                {
                    var usuarios = await usuarioRepository.GetAllAsync();
                    var tecnicos = usuarios.Where(u => u.Rol.Contains("Tecnico") || u.Rol.Contains("Técnico")).ToList();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Tecnicos.Clear();
                        foreach (var tecnico in tecnicos.OrderBy(t => t.Nombre))
                        {
                            Tecnicos.Add(tecnico);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading technicians: {ex.Message}");
            }
        }

        private void LoadCitaData(Cita cita)
        {
            ClienteId = cita.ClienteId;
            EquipoId = cita.EquipoId;
            TecnicoAsignadoId = cita.TecnicoAsignadoId;
            FechaProgramada = cita.FechaProgramada;
            HoraInicio = cita.HoraInicio;
            HoraFin = cita.HoraFin;
            TipoServicio = cita.TipoServicio;
            Estado = cita.Estado;
            Descripcion = cita.Descripcion ?? string.Empty;
            CostoEstimado = cita.CostoEstimado;

            // Cargar equipos del cliente
            _ = OnClienteChangedAsync();
        }

        private async Task OnClienteChangedAsync()
        {
            try
            {
                if (ClienteId > 0)
                {
                    var clienteConEquipos = await _clienteService.GetWithEquiposAsync(ClienteId);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        EquiposCliente.Clear();
                        if (clienteConEquipos?.Equipos != null)
                        {
                            foreach (var equipo in clienteConEquipos.Equipos.Where(e => e.Activo))
                            {
                                EquiposCliente.Add(equipo);
                            }
                        }
                    });

                    // Limpiar selección de equipo si no está en la nueva lista
                    if (EquipoId.HasValue && !EquiposCliente.Any(e => e.Id == EquipoId.Value))
                    {
                        EquipoId = null;
                    }
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        EquiposCliente.Clear();
                    });
                    EquipoId = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading client equipment: {ex.Message}");
            }
        }

        private void CalcularDuracion()
        {
            // Calcular duración automática basada en tipo de servicio
            switch (TipoServicio)
            {
                case "Mantenimiento Preventivo":
                    HoraFin = HoraInicio.Add(TimeSpan.FromHours(1.5));
                    break;
                case "Reparación":
                    HoraFin = HoraInicio.Add(TimeSpan.FromHours(2));
                    break;
                case "Instalación":
                    HoraFin = HoraInicio.Add(TimeSpan.FromHours(4));
                    break;
                case "Revisión":
                    HoraFin = HoraInicio.Add(TimeSpan.FromHours(1));
                    break;
                default:
                    HoraFin = HoraInicio.Add(TimeSpan.FromHours(2));
                    break;
            }

            OnPropertyChanged(nameof(DuracionEstimada));
        }

        private bool CanSave()
        {
            return ClienteId > 0 &&
                   !string.IsNullOrWhiteSpace(TipoServicio) &&
                   FechaProgramada >= DateTime.Today &&
                   HoraInicio >= TimeSpan.FromHours(8) &&
                   HoraInicio <= TimeSpan.FromHours(18) &&
                   !IsBusy;
        }

        private async Task SaveAsync()
        {
            try
            {
                if (!CanSave())
                {
                    MessageBox.Show("Por favor complete todos los campos requeridos correctamente.",
                                  "Validación",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                IsBusy = true;

                var cita = CreateCitaFromForm();

                if (_isEditMode)
                {
                    var result = await _citaService.UpdateAsync(cita);
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
                    var result = await _citaService.CreateAsync(cita);
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
                EquipoId = null;
                TecnicoAsignadoId = null;
                FechaProgramada = DateTime.Today.AddDays(1);
                HoraInicio = TimeSpan.FromHours(9);
                HoraFin = TimeSpan.FromHours(11);
                TipoServicio = string.Empty;
                Estado = "Programada";
                Descripcion = string.Empty;
                CostoEstimado = null;

                EquiposCliente.Clear();
            }
        }

        private Cita CreateCitaFromForm()
        {
            var cita = new Cita
            {
                ClienteId = ClienteId,
                EquipoId = EquipoId,
                TecnicoAsignadoId = TecnicoAsignadoId,
                FechaProgramada = FechaProgramada,
                HoraInicio = HoraInicio,
                HoraFin = HoraFin,
                TipoServicio = TipoServicio,
                Estado = Estado,
                Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim(),
                CostoEstimado = CostoEstimado,
                CreadoPorUsuarioId = _sessionService.CurrentUser?.Id ?? 1,
                NotificacionEnviada = false
            };

            if (_isEditMode && _originalCita != null)
            {
                cita.Id = _originalCita.Id;
                cita.FechaCreacion = _originalCita.FechaCreacion;
                cita.CostoFinal = _originalCita.CostoFinal;
                cita.NotificacionEnviada = _originalCita.NotificacionEnviada;
            }

            return cita;
        }

        private bool HasChanges()
        {
            if (!_isEditMode || _originalCita == null)
                return ClienteId > 0 ||
                       EquipoId.HasValue ||
                       TecnicoAsignadoId.HasValue ||
                       !string.IsNullOrWhiteSpace(TipoServicio) ||
                       !string.IsNullOrWhiteSpace(Descripcion) ||
                       CostoEstimado.HasValue;

            return ClienteId != _originalCita.ClienteId ||
                   EquipoId != _originalCita.EquipoId ||
                   TecnicoAsignadoId != _originalCita.TecnicoAsignadoId ||
                   FechaProgramada != _originalCita.FechaProgramada ||
                   HoraInicio != _originalCita.HoraInicio ||
                   HoraFin != _originalCita.HoraFin ||
                   TipoServicio != _originalCita.TipoServicio ||
                   Estado != _originalCita.Estado ||
                   Descripcion != (_originalCita.Descripcion ?? string.Empty) ||
                   CostoEstimado != _originalCita.CostoEstimado;
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