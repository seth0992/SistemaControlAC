using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using SistemaControlAC.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SistemaControlAC.ViewModel
{
    public class EquipoDetailViewModel : ViewModelBase
    {
        private readonly IEquipoService _equipoService;
        private readonly IClienteService _clienteService;
        private readonly ISessionService _sessionService;
        private EquipoAireAcondicionado _equipo;
        private ObservableCollection<Cita> _citas;

        public EquipoDetailViewModel(IEquipoService equipoService, IClienteService clienteService, ISessionService sessionService, EquipoAireAcondicionado equipo)
        {
            _equipoService = equipoService;
            _clienteService = clienteService;
            _sessionService = sessionService;
            _equipo = equipo;
            _citas = new ObservableCollection<Cita>();

            Title = $"Detalles del Equipo - {equipo.Marca} {equipo.Modelo}";

            // Inicializar comandos
            CloseCommand = new RelayCommand((param) => Close());
            EditEquipoCommand = new RelayCommand((param) => EditEquipo());
            PrintCommand = new RelayCommand((param) => Print());
            RefreshCommand = new RelayCommand(async (param) => await RefreshAsync());

            // Cargar datos relacionados
            _ = LoadRelatedDataAsync();
        }

        #region Propiedades

        public EquipoAireAcondicionado Equipo
        {
            get => _equipo;
            set => SetProperty(ref _equipo, value);
        }

        public ObservableCollection<Cita> Citas
        {
            get => _citas;
            set => SetProperty(ref _citas, value);
        }

        // Propiedades computadas para mostrar en la vista
        public string EquipoInfo => $"{Equipo.Marca} {Equipo.Modelo}";
        public string ClienteNombre => $"{Equipo.Cliente?.Nombre} {Equipo.Cliente?.Apellido}";
        public string TipoFormateado => Equipo.Tipo;
        public string CapacidadFormateada => string.IsNullOrWhiteSpace(Equipo.Capacidad) ? "No especificada" : Equipo.Capacidad;
        public string NumeroSerieFormateado => string.IsNullOrWhiteSpace(Equipo.NumeroSerie) ? "No especificado" : Equipo.NumeroSerie;
        public string UbicacionFormateada => Equipo.Ubicacion;
        public string EstadoFormateado => Equipo.Activo ? "Activo" : "Inactivo";
        public string FechaInstalacionFormateada => Equipo.FechaInstalacion?.ToString("dd/MM/yyyy") ?? "No especificada";
        public int TotalCitas => Citas.Count;
        public int CitasCompletadas => Citas.Count(c => c.Estado == "Completada");
        public string EdadEquipo
        {
            get
            {
                if (Equipo.FechaInstalacion.HasValue)
                {
                    var edad = DateTime.Now - Equipo.FechaInstalacion.Value;
                    var años = (int)(edad.TotalDays / 365.25);
                    var meses = (int)((edad.TotalDays % 365.25) / 30.44);

                    if (años > 0)
                        return $"{años} año{(años != 1 ? "s" : "")} y {meses} mes{(meses != 1 ? "es" : "")}";
                    else
                        return $"{meses} mes{(meses != 1 ? "es" : "")}";
                }
                return "No especificada";
            }
        }

        #endregion

        #region Comandos

        public ICommand CloseCommand { get; }
        public ICommand EditEquipoCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Métodos

        private async Task LoadRelatedDataAsync()
        {
            try
            {
                IsBusy = true;

                // Cargar equipo con datos completos
                var equipoCompleto = await _equipoService.GetWithRelationsAsync(Equipo.Id);

                if (equipoCompleto != null)
                {
                    Equipo = equipoCompleto;

                    // Notificar cambios en propiedades computadas
                    OnPropertyChanged(nameof(EquipoInfo));
                    OnPropertyChanged(nameof(ClienteNombre));
                    OnPropertyChanged(nameof(TipoFormateado));
                    OnPropertyChanged(nameof(CapacidadFormateada));
                    OnPropertyChanged(nameof(NumeroSerieFormateado));
                    OnPropertyChanged(nameof(UbicacionFormateada));
                    OnPropertyChanged(nameof(EstadoFormateado));
                    OnPropertyChanged(nameof(FechaInstalacionFormateada));
                    OnPropertyChanged(nameof(EdadEquipo));
                }

                // Cargar citas relacionadas
                // Aquí necesitarías un servicio de citas que permita buscar por equipo
                // Por ahora, simulamos una lista vacía
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Citas.Clear();
                    // TODO: Cargar citas reales del equipo cuando esté disponible el método
                    // var citasEquipo = await _citaService.GetByEquipoAsync(Equipo.Id);
                    // foreach (var cita in citasEquipo.OrderByDescending(c => c.FechaProgramada))
                    // {
                    //     Citas.Add(cita);
                    // }
                });

                // Notificar cambios en estadísticas
                OnPropertyChanged(nameof(TotalCitas));
                OnPropertyChanged(nameof(CitasCompletadas));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos relacionados: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void EditEquipo()
        {
            try
            {
                var formWindow = new View.EquipoFormWindow();
                var formViewModel = new EquipoFormViewModel(_equipoService, _clienteService, _sessionService, Equipo);
                formWindow.DataContext = formViewModel;
                formWindow.Owner = GetCurrentWindow();

                if (formWindow.ShowDialog() == true)
                {
                    _ = RefreshAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el formulario de edición: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void Print()
        {
            try
            {
                MessageBox.Show("Funcionalidad de impresión no implementada aún.",
                              "Información",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private async Task RefreshAsync()
        {
            try
            {
                // Recargar datos del equipo
                var equipoActualizado = await _equipoService.GetWithRelationsAsync(Equipo.Id);
                if (equipoActualizado != null)
                {
                    Equipo = equipoActualizado;
                    Title = $"Detalles del Equipo - {Equipo.Marca} {Equipo.Modelo}";

                    // Notificar cambios en todas las propiedades computadas
                    OnPropertyChanged(nameof(EquipoInfo));
                    OnPropertyChanged(nameof(ClienteNombre));
                    OnPropertyChanged(nameof(TipoFormateado));
                    OnPropertyChanged(nameof(CapacidadFormateada));
                    OnPropertyChanged(nameof(NumeroSerieFormateado));
                    OnPropertyChanged(nameof(UbicacionFormateada));
                    OnPropertyChanged(nameof(EstadoFormateado));
                    OnPropertyChanged(nameof(FechaInstalacionFormateada));
                    OnPropertyChanged(nameof(EdadEquipo));
                }

                // Recargar datos relacionados
                await LoadRelatedDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar los datos: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void Close()
        {
            var window = GetCurrentWindow();
            window?.Close();
        }

        private Window? GetCurrentWindow()
        {
            return Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
        }

        #endregion
    }
}