using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using SistemaControlAC.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SistemaControlAC.ViewModel
{
    public class ClienteDetailViewModel : ViewModelBase
    {
        private readonly IClienteService _clienteService;
        private readonly ISessionService _sessionService;
        private Cliente _cliente;
        private ObservableCollection<EquipoAireAcondicionado> _equipos;
        private ObservableCollection<Cita> _citas;

        public ClienteDetailViewModel(IClienteService clienteService, ISessionService sessionService, Cliente cliente)
        {
            _clienteService = clienteService;
            _sessionService = sessionService;
            _cliente = cliente;
            _equipos = new ObservableCollection<EquipoAireAcondicionado>();
            _citas = new ObservableCollection<Cita>();

            Title = $"Detalles del Cliente - {cliente.Nombre} {cliente.Apellido}";

            // Inicializar comandos
            CloseCommand = new RelayCommand((param) => Close());
            EditClienteCommand = new RelayCommand((param) => EditCliente());
            PrintCommand = new RelayCommand((param) => Print());
            RefreshCommand = new RelayCommand(async (param) => await RefreshAsync());

            // Cargar datos relacionados
            _ = LoadRelatedDataAsync();
        }

        #region Propiedades

        public Cliente Cliente
        {
            get => _cliente;
            set => SetProperty(ref _cliente, value);
        }

        public ObservableCollection<EquipoAireAcondicionado> Equipos
        {
            get => _equipos;
            set => SetProperty(ref _equipos, value);
        }

        public ObservableCollection<Cita> Citas
        {
            get => _citas;
            set => SetProperty(ref _citas, value);
        }

        // Propiedades computadas para mostrar en la vista
        public string NombreCompleto => $"{Cliente.Nombre} {Cliente.Apellido}";

        public string TelefonoFormateado => Cliente.Telefono;

        public string DireccionCompleta
        {
            get
            {
                var direccion = Cliente.Direccion;
                if (!string.IsNullOrWhiteSpace(Cliente.Ciudad))
                {
                    direccion += $", {Cliente.Ciudad}";
                }
                if (!string.IsNullOrWhiteSpace(Cliente.CodigoPostal))
                {
                    direccion += $" {Cliente.CodigoPostal}";
                }
                return direccion;
            }
        }

        public string EstadoFormateado => Cliente.Activo ? "Activo" : "Inactivo";

        public string FechaRegistroFormateada => Cliente.FechaRegistro.ToString("dd/MM/yyyy HH:mm");

        public string NotasFormateadas => string.IsNullOrWhiteSpace(Cliente.Notas) ? "Sin notas" : Cliente.Notas;

        public int TotalEquipos => Equipos.Count;
        public int EquiposActivos => Equipos.Count(e => e.Activo);
        public int TotalCitas => Citas.Count;

        #endregion

        #region Comandos

        public ICommand CloseCommand { get; }
        public ICommand EditClienteCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Métodos

        private async Task LoadRelatedDataAsync()
        {
            try
            {
                IsBusy = true;

                // Cargar cliente con equipos relacionados
                var clienteConEquipos = await _clienteService.GetWithEquiposAsync(Cliente.Id);

                if (clienteConEquipos != null)
                {
                    Cliente = clienteConEquipos;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Equipos.Clear();
                        if (clienteConEquipos.Equipos != null)
                        {
                            foreach (var equipo in clienteConEquipos.Equipos.OrderBy(e => e.Ubicacion))
                            {
                                Equipos.Add(equipo);
                            }
                        }

                        Citas.Clear();
                        if (clienteConEquipos.Citas != null)
                        {
                            foreach (var cita in clienteConEquipos.Citas.OrderByDescending(c => c.FechaProgramada))
                            {
                                Citas.Add(cita);
                            }
                        }
                    });
                }

                // Notificar cambios en propiedades computadas
                OnPropertyChanged(nameof(TotalEquipos));
                OnPropertyChanged(nameof(EquiposActivos));
                OnPropertyChanged(nameof(TotalCitas));
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

        private void EditCliente()
        {
            try
            {
                var formWindow = new View.ClienteFormWindow();
                var formViewModel = new ClienteFormViewModel(_clienteService, _sessionService, Cliente);
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
                // Aquí implementarías la funcionalidad de impresión
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
                // Recargar datos del cliente
                var clienteActualizado = await _clienteService.GetByIdAsync(Cliente.Id);
                if (clienteActualizado != null)
                {
                    Cliente = clienteActualizado;
                    Title = $"Detalles del Cliente - {Cliente.Nombre} {Cliente.Apellido}";

                    // Notificar cambios en todas las propiedades computadas
                    OnPropertyChanged(nameof(NombreCompleto));
                    OnPropertyChanged(nameof(TelefonoFormateado));
                    OnPropertyChanged(nameof(DireccionCompleta));
                    OnPropertyChanged(nameof(EstadoFormateado));
                    OnPropertyChanged(nameof(NotasFormateadas));
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