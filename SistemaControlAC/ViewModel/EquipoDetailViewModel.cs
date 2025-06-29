using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using SistemaControlAC.Utilities;
using SistemaControlAC.View;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SistemaControlAC.ViewModel
{
    public class EquipoDetailViewModel : ViewModelBase
    {
        private readonly IEquipoService _equipoService;
        private readonly ISessionService _sessionService;
        private EquipoAireAcondicionado _equipo;

        public EquipoDetailViewModel(IEquipoService equipoService, ISessionService sessionService, EquipoAireAcondicionado equipo)
        {
            _equipoService = equipoService;
            _sessionService = sessionService;
            _equipo = equipo;

            Title = $"Detalles del Equipo - {equipo.Marca} {equipo.Modelo}";

            // Inicializar comandos
            CloseCommand = new RelayCommand((param) => Close());
            EditEquipoCommand = new RelayCommand((param) => EditEquipo());
            PrintCommand = new RelayCommand((param) => Print());
            RefreshCommand = new RelayCommand(async (param) => await RefreshAsync());

            // Cargar datos completos
            _ = LoadEquipoDetailsAsync();
        }

        #region Propiedades

        public EquipoAireAcondicionado Equipo
        {
            get => _equipo;
            set => SetProperty(ref _equipo, value);
        }

        // Propiedades computadas para mostrar en la vista
        public string ClienteNombre => $"{Equipo.Cliente?.Nombre} {Equipo.Cliente?.Apellido}";
        public string EquipoCompleto => $"{Equipo.Marca} {Equipo.Modelo}";
        public string TipoFormateado => Equipo.Tipo;
        public string UbicacionFormateada => Equipo.Ubicacion;
        public string NumeroSerieFormateado => string.IsNullOrWhiteSpace(Equipo.NumeroSerie) ? "No especificado" : Equipo.NumeroSerie;
        public string CapacidadFormateada => string.IsNullOrWhiteSpace(Equipo.Capacidad) ? "No especificada" : Equipo.Capacidad;
        public string FechaInstalacionFormateada => Equipo.FechaInstalacion?.ToString("dd/MM/yyyy") ?? "No especificada";
        public string EstadoFormateado => Equipo.Activo ? "Activo" : "Inactivo";

        #endregion

        #region Comandos

        public ICommand CloseCommand { get; }
        public ICommand EditEquipoCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Métodos

        private async Task LoadEquipoDetailsAsync()
        {
            try
            {
                IsBusy = true;

                // Cargar equipo con cliente relacionado
                var equipoCompleto = await _equipoService.GetWithClienteAsync(Equipo.Id);

                if (equipoCompleto != null)
                {
                    Equipo = equipoCompleto;
                    Title = $"Detalles del Equipo - {Equipo.Marca} {Equipo.Modelo}";

                    // Notificar cambios en propiedades computadas
                    OnPropertyChanged(nameof(ClienteNombre));
                    OnPropertyChanged(nameof(EquipoCompleto));
                    OnPropertyChanged(nameof(TipoFormateado));
                    OnPropertyChanged(nameof(UbicacionFormateada));
                    OnPropertyChanged(nameof(NumeroSerieFormateado));
                    OnPropertyChanged(nameof(CapacidadFormateada));
                    OnPropertyChanged(nameof(FechaInstalacionFormateada));
                    OnPropertyChanged(nameof(EstadoFormateado));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los detalles del equipo: {ex.Message}",
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
                var formWindow = new EquipoFormWindow();
                var formViewModel = new EquipoFormViewModel(_equipoService, _sessionService, Equipo);
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
                var equipoActualizado = await _equipoService.GetWithClienteAsync(Equipo.Id);
                if (equipoActualizado != null)
                {
                    Equipo = equipoActualizado;
                    Title = $"Detalles del Equipo - {Equipo.Marca} {Equipo.Modelo}";

                    // Notificar cambios en propiedades computadas
                    OnPropertyChanged(nameof(ClienteNombre));
                    OnPropertyChanged(nameof(EquipoCompleto));
                    OnPropertyChanged(nameof(TipoFormateado));
                    OnPropertyChanged(nameof(UbicacionFormateada));
                    OnPropertyChanged(nameof(NumeroSerieFormateado));
                    OnPropertyChanged(nameof(CapacidadFormateada));
                    OnPropertyChanged(nameof(FechaInstalacionFormateada));
                    OnPropertyChanged(nameof(EstadoFormateado));
                }
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