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
    public class EquipoViewModel : ViewModelBase
    {
        private readonly IEquipoService _equipoService;
        private readonly ISessionService _sessionService;

        private ObservableCollection<EquipoAireAcondicionado> _equipos;
        private EquipoAireAcondicionado? _selectedEquipo;
        private string _searchText = string.Empty;
        private bool _showInactive = false;
        private int _totalEquipos;
        private int _activeEquipos;

        public EquipoViewModel(IEquipoService equipoService, ISessionService sessionService)
        {
            _equipoService = equipoService;
            _sessionService = sessionService;
            _equipos = new ObservableCollection<EquipoAireAcondicionado>();

            Title = "Gestión de Equipos";

            // Configurar vista de colección
            EquiposView = CollectionViewSource.GetDefaultView(_equipos);
            EquiposView.Filter = FilterEquipos;

            // Inicializar comandos
            LoadDataCommand = new RelayCommand(async (param) => await LoadDataAsync());
            SearchCommand = new RelayCommand(async (param) => await SearchAsync());
            AddEquipoCommand = new RelayCommand((param) => AddEquipo());
            EditEquipoCommand = new RelayCommand((param) => EditEquipo(), (param) => SelectedEquipo != null);
            DeleteEquipoCommand = new RelayCommand(async (param) => await DeleteEquipoAsync(), (param) => SelectedEquipo != null);
            ViewEquipoCommand = new RelayCommand((param) => ViewEquipo(), (param) => SelectedEquipo != null);
            RefreshCommand = new RelayCommand(async (param) => await RefreshAsync());
            ToggleInactiveCommand = new RelayCommand(async (param) => await ToggleInactiveAsync());

            // Cargar datos iniciales
            _ = LoadDataAsync();
        }

        #region Propiedades

        public ObservableCollection<EquipoAireAcondicionado> Equipos
        {
            get => _equipos;
            set => SetProperty(ref _equipos, value);
        }

        public ICollectionView EquiposView { get; }

        public EquipoAireAcondicionado? SelectedEquipo
        {
            get => _selectedEquipo;
            set => SetProperty(ref _selectedEquipo, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    EquiposView.Refresh();
                }
            }
        }

        public bool ShowInactive
        {
            get => _showInactive;
            set
            {
                if (SetProperty(ref _showInactive, value))
                {
                    _ = LoadDataAsync();
                }
            }
        }

        public int TotalEquipos
        {
            get => _totalEquipos;
            set => SetProperty(ref _totalEquipos, value);
        }

        public int ActiveEquipos
        {
            get => _activeEquipos;
            set => SetProperty(ref _activeEquipos, value);
        }

        #endregion

        #region Comandos

        public ICommand LoadDataCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddEquipoCommand { get; }
        public ICommand EditEquipoCommand { get; }
        public ICommand DeleteEquipoCommand { get; }
        public ICommand ViewEquipoCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ToggleInactiveCommand { get; }

        #endregion

        #region Métodos

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;

                List<EquipoAireAcondicionado> equipos;
                if (ShowInactive)
                {
                    equipos = await _equipoService.GetAllAsync();
                }
                else
                {
                    equipos = await _equipoService.GetActiveAsync();
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Equipos.Clear();
                    foreach (var equipo in equipos)
                    {
                        Equipos.Add(equipo);
                    }
                });

                // Actualizar estadísticas
                TotalEquipos = await _equipoService.CountAsync();
                ActiveEquipos = await _equipoService.CountActiveAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los equipos: {ex.Message}",
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

                var resultados = await _equipoService.SearchAsync(SearchText);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Equipos.Clear();
                    foreach (var equipo in resultados)
                    {
                        Equipos.Add(equipo);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar equipos: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddEquipo()
        {
            try
            {
                var formWindow = new View.EquipoFormWindow();
                var formViewModel = new EquipoFormViewModel(_equipoService, _sessionService);
                formWindow.DataContext = formViewModel;
                formWindow.Owner = Application.Current.MainWindow;

                if (formWindow.ShowDialog() == true)
                {
                    _ = RefreshAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el formulario de equipo: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void EditEquipo()
        {
            if (SelectedEquipo == null)
                return;

            try
            {
                var formWindow = new View.EquipoFormWindow();
                var formViewModel = new EquipoFormViewModel(_equipoService, _sessionService, SelectedEquipo);
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

        private async Task DeleteEquipoAsync()
        {
            if (SelectedEquipo == null)
                return;

            try
            {
                var result = MessageBox.Show(
                    $"¿Está seguro que desea eliminar el equipo {SelectedEquipo.Marca} {SelectedEquipo.Modelo}?\n\n" +
                    "Esta acción marcará el equipo como inactivo.",
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;

                    var deleteResult = await _equipoService.DeleteAsync(SelectedEquipo.Id);

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
                MessageBox.Show($"Error al eliminar el equipo: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ViewEquipo()
        {
            if (SelectedEquipo == null)
                return;

            try
            {
                var detailWindow = new View.EquipoDetailWindow();
                var detailViewModel = new EquipoDetailViewModel(_equipoService, _sessionService, SelectedEquipo);
                detailWindow.DataContext = detailViewModel;
                detailWindow.Owner = Application.Current.MainWindow;
                detailWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar los detalles del equipo: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private async Task RefreshAsync()
        {
            SelectedEquipo = null;
            SearchText = string.Empty;
            await LoadDataAsync();
        }

        private async Task ToggleInactiveAsync()
        {
            await LoadDataAsync();
        }

        private bool FilterEquipos(object obj)
        {
            if (obj is not EquipoAireAcondicionado equipo)
                return false;

            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            var searchTerm = SearchText.ToLower();

            return equipo.Marca.ToLower().Contains(searchTerm) ||
                   equipo.Modelo.ToLower().Contains(searchTerm) ||
                   equipo.Tipo.ToLower().Contains(searchTerm) ||
                   equipo.Ubicacion.ToLower().Contains(searchTerm) ||
                   (!string.IsNullOrWhiteSpace(equipo.NumeroSerie) && equipo.NumeroSerie.ToLower().Contains(searchTerm)) ||
                   (equipo.Cliente != null &&
                    (equipo.Cliente.Nombre.ToLower().Contains(searchTerm) ||
                     equipo.Cliente.Apellido.ToLower().Contains(searchTerm)));
        }

        #endregion
    }
}