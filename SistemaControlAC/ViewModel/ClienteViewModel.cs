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
    public class ClienteViewModel : ViewModelBase
    {
        private readonly IClienteService _clienteService;
        private readonly ISessionService _sessionService;

        private ObservableCollection<Cliente> _clientes;
        private Cliente? _selectedCliente;
        private string _searchText = string.Empty;
        private bool _showInactive = false;
        private int _totalClientes;
        private int _activeClientes;

        public ClienteViewModel(IClienteService clienteService, ISessionService sessionService)
        {
            _clienteService = clienteService;
            _sessionService = sessionService;
            _clientes = new ObservableCollection<Cliente>();

            Title = "Gestión de Clientes";

            // Configurar vista de colección
            ClientesView = CollectionViewSource.GetDefaultView(_clientes);
            ClientesView.Filter = FilterClientes;

            // Inicializar comandos
            LoadDataCommand = new RelayCommand(async (param) => await LoadDataAsync());
            SearchCommand = new RelayCommand(async (param) => await SearchAsync());
            AddClienteCommand = new RelayCommand((param) => AddCliente());
            EditClienteCommand = new RelayCommand((param) => EditCliente(), (param) => SelectedCliente != null);
            DeleteClienteCommand = new RelayCommand(async (param) => await DeleteClienteAsync(), (param) => SelectedCliente != null);
            ViewClienteCommand = new RelayCommand((param) => ViewCliente(), (param) => SelectedCliente != null);
            RefreshCommand = new RelayCommand(async (param) => await RefreshAsync());
            ToggleInactiveCommand = new RelayCommand(async (param) => await ToggleInactiveAsync());

            // Cargar datos iniciales
            _ = LoadDataAsync();
        }

        #region Propiedades

        public ObservableCollection<Cliente> Clientes
        {
            get => _clientes;
            set => SetProperty(ref _clientes, value);
        }

        public ICollectionView ClientesView { get; }

        public Cliente? SelectedCliente
        {
            get => _selectedCliente;
            set => SetProperty(ref _selectedCliente, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ClientesView.Refresh();
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

        public int TotalClientes
        {
            get => _totalClientes;
            set => SetProperty(ref _totalClientes, value);
        }

        public int ActiveClientes
        {
            get => _activeClientes;
            set => SetProperty(ref _activeClientes, value);
        }

        #endregion

        #region Comandos

        public ICommand LoadDataCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddClienteCommand { get; }
        public ICommand EditClienteCommand { get; }
        public ICommand DeleteClienteCommand { get; }
        public ICommand ViewClienteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ToggleInactiveCommand { get; }

        #endregion

        #region Métodos

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;

                List<Cliente> clientes;
                if (ShowInactive)
                {
                    clientes = await _clienteService.GetAllAsync();
                }
                else
                {
                    clientes = await _clienteService.GetActiveAsync();
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Clientes.Clear();
                    foreach (var cliente in clientes)
                    {
                        Clientes.Add(cliente);
                    }
                });

                // Actualizar estadísticas
                TotalClientes = await _clienteService.CountAsync();
                ActiveClientes = await _clienteService.CountActiveAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los clientes: {ex.Message}",
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

                var resultados = await _clienteService.SearchAsync(SearchText);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Clientes.Clear();
                    foreach (var cliente in resultados)
                    {
                        Clientes.Add(cliente);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar clientes: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddCliente()
        {
            try
            {
                var formWindow = new View.ClienteFormWindow();
                var formViewModel = new ClienteFormViewModel(_clienteService, _sessionService);
                formWindow.DataContext = formViewModel;
                formWindow.Owner = Application.Current.MainWindow;

                if (formWindow.ShowDialog() == true)
                {
                    _ = RefreshAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el formulario de cliente: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void EditCliente()
        {
            if (SelectedCliente == null)
                return;

            try
            {
                var formWindow = new View.ClienteFormWindow();
                var formViewModel = new ClienteFormViewModel(_clienteService, _sessionService, SelectedCliente);
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

        private async Task DeleteClienteAsync()
        {
            if (SelectedCliente == null)
                return;

            try
            {
                var result = MessageBox.Show(
                    $"¿Está seguro que desea eliminar el cliente {SelectedCliente.Nombre} {SelectedCliente.Apellido}?\n\n" +
                    "Esta acción marcará el cliente como inactivo.",
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;

                    var deleteResult = await _clienteService.DeleteAsync(SelectedCliente.Id);

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
                MessageBox.Show($"Error al eliminar el cliente: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ViewCliente()
        {
            if (SelectedCliente == null)
                return;

            try
            {
                var detailWindow = new View.ClienteDetailWindow();
                var detailViewModel = new ClienteDetailViewModel(_clienteService, _sessionService, SelectedCliente);
                detailWindow.DataContext = detailViewModel;
                detailWindow.Owner = Application.Current.MainWindow;
                detailWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar los detalles del cliente: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private async Task RefreshAsync()
        {
            SelectedCliente = null;
            SearchText = string.Empty;
            await LoadDataAsync();
        }

        private async Task ToggleInactiveAsync()
        {
            await LoadDataAsync();
        }

        private bool FilterClientes(object obj)
        {
            if (obj is not Cliente cliente)
                return false;

            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            var searchTerm = SearchText.ToLower();

            return cliente.Nombre.ToLower().Contains(searchTerm) ||
                   cliente.Apellido.ToLower().Contains(searchTerm) ||
                   cliente.Telefono.Contains(searchTerm) ||
                   (!string.IsNullOrWhiteSpace(cliente.Email) && cliente.Email.ToLower().Contains(searchTerm)) ||
                   cliente.Direccion.ToLower().Contains(searchTerm);
        }

        #endregion
    }
}
