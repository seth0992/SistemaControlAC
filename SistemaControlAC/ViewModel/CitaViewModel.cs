using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using SistemaControlAC.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SistemaControlAC.ViewModel
{
    public class CitaViewModel : ViewModelBase
    {
        private readonly ICitaService _citaService;
        private readonly ISessionService _sessionService;

        private ObservableCollection<Cita> _citas;
        private Cita? _selectedCita;
        private string _searchText = string.Empty;
        private string _estadoFiltro = "Todas";
        private DateTime _fechaFiltro = DateTime.Today;
        private bool _mostrarSoloHoy = false;

        // Estadísticas
        private int _totalCitas;
        private int _citasProgramadas;
        private int _citasEnProceso;
        private int _citasCompletadas;
        private int _citasCanceladas;

        public CitaViewModel(ICitaService citaService, ISessionService sessionService)
        {
            _citaService = citaService;
            _sessionService = sessionService;
            _citas = new ObservableCollection<Cita>();

            Title = "Gestión de Citas";

            // Configurar vista de colección
            CitasView = CollectionViewSource.GetDefaultView(_citas);
            CitasView.Filter = FilterCitas;

            // Inicializar comandos
            LoadDataCommand = new RelayCommand(async (param) => await LoadDataAsync());
            SearchCommand = new RelayCommand(async (param) => await SearchAsync());
            AddCitaCommand = new RelayCommand((param) => AddCita());
            EditCitaCommand = new RelayCommand((param) => EditCita(), (param) => SelectedCita != null);
            DeleteCitaCommand = new RelayCommand(async (param) => await DeleteCitaAsync(), (param) => SelectedCita != null);
            ViewCitaCommand = new RelayCommand((param) => ViewCita(), (param) => SelectedCita != null);
            CompletarCitaCommand = new RelayCommand(async (param) => await CompletarCitaAsync(), (param) => CanCompletarCita());
            CancelarCitaCommand = new RelayCommand(async (param) => await CancelarCitaAsync(), (param) => CanCancelarCita());
            RefreshCommand = new RelayCommand(async (param) => await RefreshAsync());
            ToggleFiltroHoyCommand = new RelayCommand(async (param) => await ToggleFiltroHoyAsync());

            // Cargar datos iniciales
            _ = LoadDataAsync();
        }

        #region Propiedades

        public ObservableCollection<Cita> Citas
        {
            get => _citas;
            set => SetProperty(ref _citas, value);
        }

        public ICollectionView CitasView { get; }

        public Cita? SelectedCita
        {
            get => _selectedCita;
            set => SetProperty(ref _selectedCita, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    CitasView.Refresh();
                }
            }
        }

        public string EstadoFiltro
        {
            get => _estadoFiltro;
            set
            {
                if (SetProperty(ref _estadoFiltro, value))
                {
                    CitasView.Refresh();
                }
            }
        }

        public DateTime FechaFiltro
        {
            get => _fechaFiltro;
            set
            {
                if (SetProperty(ref _fechaFiltro, value))
                {
                    if (MostrarSoloHoy)
                    {
                        _ = LoadDataAsync();
                    }
                }
            }
        }

        public bool MostrarSoloHoy
        {
            get => _mostrarSoloHoy;
            set
            {
                if (SetProperty(ref _mostrarSoloHoy, value))
                {
                    _ = LoadDataAsync();
                }
            }
        }

        // Estadísticas
        public int TotalCitas
        {
            get => _totalCitas;
            set => SetProperty(ref _totalCitas, value);
        }

        public int CitasProgramadas
        {
            get => _citasProgramadas;
            set => SetProperty(ref _citasProgramadas, value);
        }

        public int CitasEnProceso
        {
            get => _citasEnProceso;
            set => SetProperty(ref _citasEnProceso, value);
        }

        public int CitasCompletadas
        {
            get => _citasCompletadas;
            set => SetProperty(ref _citasCompletadas, value);
        }

        public int CitasCanceladas
        {
            get => _citasCanceladas;
            set => SetProperty(ref _citasCanceladas, value);
        }

        // Lista de estados para el filtro
        public List<string> EstadosFiltro => new List<string>
        {
            "Todas",
            "Programada",
            "En Proceso",
            "Completada",
            "Cancelada"
        };

        #endregion

        #region Comandos

        public ICommand LoadDataCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddCitaCommand { get; }
        public ICommand EditCitaCommand { get; }
        public ICommand DeleteCitaCommand { get; }
        public ICommand ViewCitaCommand { get; }
        public ICommand CompletarCitaCommand { get; }
        public ICommand CancelarCitaCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ToggleFiltroHoyCommand { get; }

        #endregion

        #region Métodos

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;

                List<Cita> citas;
                if (MostrarSoloHoy)
                {
                    citas = await _citaService.GetByFechaAsync(FechaFiltro);
                }
                else
                {
                    citas = await _citaService.GetAllAsync();
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Citas.Clear();
                    foreach (var cita in citas)
                    {
                        Citas.Add(cita);
                    }
                });

                // Actualizar estadísticas
                await UpdateStatisticsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las citas: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SearchAsync()
        {
            try
            {
                IsBusy = true;

                var resultados = await _citaService.SearchAsync(SearchText);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Citas.Clear();
                    foreach (var cita in resultados)
                    {
                        Citas.Add(cita);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar citas: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddCita()
        {
            try
            {
                var formWindow = new View.CitaFormWindow();
                var formViewModel = new CitaFormViewModel(_citaService, _sessionService);
                formWindow.DataContext = formViewModel;
                formWindow.Owner = Application.Current.MainWindow;

                if (formWindow.ShowDialog() == true)
                {
                    _ = RefreshAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el formulario de cita: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void EditCita()
        {
            if (SelectedCita == null)
                return;

            try
            {
                var formWindow = new View.CitaFormWindow();
                var formViewModel = new CitaFormViewModel(_citaService, _sessionService, SelectedCita);
                formWindow.DataContext = formViewModel;
                formWindow.Owner = Application.Current.MainWindow;

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

        private async Task DeleteCitaAsync()
        {
            if (SelectedCita == null)
                return;

            try
            {
                var result = MessageBox.Show(
                    $"¿Está seguro que desea eliminar la cita del {SelectedCita.FechaProgramada:dd/MM/yyyy} a las {SelectedCita.HoraInicio}?\n\n" +
                    $"Cliente: {SelectedCita.Cliente?.Nombre} {SelectedCita.Cliente?.Apellido}\n" +
                    "Esta acción marcará la cita como cancelada.",
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;

                    var deleteResult = await _citaService.DeleteAsync(SelectedCita.Id);

                    if (deleteResult.Success)
                    {
                        MessageBox.Show(deleteResult.Message,
                                      "Éxito",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        await RefreshAsync();
                    }
                    else
                    {
                        MessageBox.Show(deleteResult.Message,
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar la cita: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ViewCita()
        {
            if (SelectedCita == null)
                return;

            try
            {
                var detailWindow = new View.CitaDetailWindow();
                var detailViewModel = new CitaDetailViewModel(_citaService, _sessionService, SelectedCita);
                detailWindow.DataContext = detailViewModel;
                detailWindow.Owner = Application.Current.MainWindow;
                detailWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar los detalles de la cita: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private async Task CompletarCitaAsync()
        {
            if (SelectedCita == null)
                return;

            try
            {
                var result = MessageBox.Show(
                    $"¿Está seguro que desea marcar como completada la cita del {SelectedCita.FechaProgramada:dd/MM/yyyy}?",
                    "Confirmar Completar Cita",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;

                    var completarResult = await _citaService.CompletarCitaAsync(SelectedCita.Id);

                    if (completarResult.Success)
                    {
                        MessageBox.Show(completarResult.Message,
                                      "Éxito",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        await RefreshAsync();
                    }
                    else
                    {
                        MessageBox.Show(completarResult.Message,
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al completar la cita: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CancelarCitaAsync()
        {
            if (SelectedCita == null)
                return;

            try
            {
                // Crear un simple diálogo para el motivo
                var motivo = Microsoft.VisualBasic.Interaction.InputBox(
                    "Ingrese el motivo de cancelación:",
                    "Cancelar Cita",
                    "");

                if (!string.IsNullOrEmpty(motivo))
                {
                    IsBusy = true;

                    var cancelarResult = await _citaService.CancelAsync(SelectedCita.Id, motivo);

                    if (cancelarResult.Success)
                    {
                        MessageBox.Show(cancelarResult.Message,
                                      "Éxito",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        await RefreshAsync();
                    }
                    else
                    {
                        MessageBox.Show(cancelarResult.Message,
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cancelar la cita: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshAsync()
        {
            SelectedCita = null;
            SearchText = string.Empty;
            await LoadDataAsync();
        }

        private async Task ToggleFiltroHoyAsync()
        {
            await LoadDataAsync();
        }

        private async Task UpdateStatisticsAsync()
        {
            try
            {
                TotalCitas = await _citaService.CountAsync();
                CitasProgramadas = await _citaService.CountByEstadoAsync("Programada");
                CitasEnProceso = await _citaService.CountByEstadoAsync("En Proceso");
                CitasCompletadas = await _citaService.CountByEstadoAsync("Completada");
                CitasCanceladas = await _citaService.CountByEstadoAsync("Cancelada");
            }
            catch (Exception ex)
            {
                // Log error but don't show to user
                System.Diagnostics.Debug.WriteLine($"Error updating statistics: {ex.Message}");
            }
        }

        private bool FilterCitas(object obj)
        {
            if (obj is not Cita cita)
                return false;

            // Filtro por texto de búsqueda
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchTerm = SearchText.ToLower();

                var matchesSearch =
                    (cita.Cliente?.Nombre?.ToLower().Contains(searchTerm) ?? false) ||
                    (cita.Cliente?.Apellido?.ToLower().Contains(searchTerm) ?? false) ||
                    cita.TipoServicio.ToLower().Contains(searchTerm) ||
                    cita.Estado.ToLower().Contains(searchTerm) ||
                    (cita.Descripcion?.ToLower().Contains(searchTerm) ?? false) ||
                    (cita.TecnicoAsignado?.Nombre?.ToLower().Contains(searchTerm) ?? false) ||
                    (cita.TecnicoAsignado?.Apellido?.ToLower().Contains(searchTerm) ?? false);

                if (!matchesSearch)
                    return false;
            }

            // Filtro por estado
            if (EstadoFiltro != "Todas" && cita.Estado != EstadoFiltro)
                return false;

            return true;
        }

        private bool CanCompletarCita()
        {
            return SelectedCita != null &&
                   (SelectedCita.Estado == "Programada" || SelectedCita.Estado == "En Proceso");
        }

        private bool CanCancelarCita()
        {
            return SelectedCita != null &&
                   SelectedCita.Estado != "Completada" &&
                   SelectedCita.Estado != "Cancelada";
        }

        #endregion
    }
}