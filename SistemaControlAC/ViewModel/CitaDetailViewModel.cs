using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using SistemaControlAC.Utilities;
using SistemaControlAC.View;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SistemaControlAC.ViewModel
{
    public class CitaDetailViewModel : ViewModelBase
    {
        private readonly ICitaService _citaService;
        private readonly ISessionService _sessionService;
        private Cita _cita;

        public CitaDetailViewModel(ICitaService citaService, ISessionService sessionService, Cita cita)
        {
            _citaService = citaService;
            _sessionService = sessionService;
            _cita = cita;

            Title = $"Detalles de la Cita - {cita.FechaProgramada:dd/MM/yyyy}";

            // Inicializar comandos
            CloseCommand = new RelayCommand((param) => Close());
            EditCitaCommand = new RelayCommand((param) => EditCita());
            PrintCommand = new RelayCommand((param) => Print());
            RefreshCommand = new RelayCommand(async (param) => await RefreshAsync());
            CompletarCitaCommand = new RelayCommand(async (param) => await CompletarCitaAsync(), (param) => CanCompletar());
            CancelarCitaCommand = new RelayCommand(async (param) => await CancelarCitaAsync(), (param) => CanCancelar());

            // Cargar datos completos
            _ = LoadCitaDetailsAsync();
        }

        #region Propiedades

        public Cita Cita
        {
            get => _cita;
            set => SetProperty(ref _cita, value);
        }

        // Propiedades computadas para mostrar en la vista
        public string ClienteNombre => $"{Cita.Cliente?.Nombre} {Cita.Cliente?.Apellido}";
        public string TecnicoNombre => $"{Cita.TecnicoAsignado?.Nombre} {Cita.TecnicoAsignado?.Apellido}";
        public string EquipoInfo => Cita.Equipo != null ? $"{Cita.Equipo.Marca} {Cita.Equipo.Modelo}" : "No especificado";
        public string FechaFormateada => Cita.FechaProgramada.ToString("dddd, dd MMMM yyyy");
        public string HorarioFormateado => $"{Cita.HoraInicio:hh\\:mm} - {Cita.HoraFin:hh\\:mm}";
        public string DuracionCalculada
        {
            get
            {
                if (Cita.HoraFin.HasValue)
                {
                    var duracion = Cita.HoraFin.Value - Cita.HoraInicio;
                    return $"{duracion.Hours}h {duracion.Minutes}min";
                }
                return "No definida";
            }
        }
        public string CostoFormateado => Cita.CostoEstimado?.ToString("C") ?? "No definido";
        public string EstadoFormateado => Cita.Estado;
        public string DescripcionFormateada => string.IsNullOrWhiteSpace(Cita.Descripcion) ? "Sin descripción" : Cita.Descripcion;

        #endregion

        #region Comandos

        public ICommand CloseCommand { get; }
        public ICommand EditCitaCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CompletarCitaCommand { get; }
        public ICommand CancelarCitaCommand { get; }

        #endregion

        #region Métodos

        private async Task LoadCitaDetailsAsync()
        {
            try
            {
                IsBusy = true;

                // Cargar cita con todas las relaciones
                var citaCompleta = await _citaService.GetByIdAsync(Cita.Id);

                if (citaCompleta != null)
                {
                    Cita = citaCompleta;
                    Title = $"Detalles de la Cita - {Cita.FechaProgramada:dd/MM/yyyy}";

                    // Notificar cambios en propiedades computadas
                    OnPropertyChanged(nameof(ClienteNombre));
                    OnPropertyChanged(nameof(TecnicoNombre));
                    OnPropertyChanged(nameof(EquipoInfo));
                    OnPropertyChanged(nameof(FechaFormateada));
                    OnPropertyChanged(nameof(HorarioFormateado));
                    OnPropertyChanged(nameof(DuracionCalculada));
                    OnPropertyChanged(nameof(CostoFormateado));
                    OnPropertyChanged(nameof(EstadoFormateado));
                    OnPropertyChanged(nameof(DescripcionFormateada));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los detalles de la cita: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void EditCita()
        {
            try
            {
                var formWindow = new CitaFormWindow();
                var formViewModel = new CitaFormViewModel(_citaService, _sessionService, Cita);
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
                var citaActualizada = await _citaService.GetByIdAsync(Cita.Id);
                if (citaActualizada != null)
                {
                    Cita = citaActualizada;

                    // Notificar cambios en propiedades computadas
                    OnPropertyChanged(nameof(ClienteNombre));
                    OnPropertyChanged(nameof(TecnicoNombre));
                    OnPropertyChanged(nameof(EquipoInfo));
                    OnPropertyChanged(nameof(FechaFormateada));
                    OnPropertyChanged(nameof(HorarioFormateado));
                    OnPropertyChanged(nameof(DuracionCalculada));
                    OnPropertyChanged(nameof(CostoFormateado));
                    OnPropertyChanged(nameof(EstadoFormateado));
                    OnPropertyChanged(nameof(DescripcionFormateada));
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

        private async Task CompletarCitaAsync()
        {
            try
            {
                var result = MessageBox.Show(
                    $"¿Está seguro que desea marcar como completada la cita del {Cita.FechaProgramada:dd/MM/yyyy}?",
                    "Confirmar Completar Cita",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;

                    var completarResult = await _citaService.CompletarCitaAsync(Cita.Id);

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
            try
            {
                // Crear ventana simple para solicitar motivo
                var motivoDialog = new MotivoDialog();
                motivoDialog.Owner = GetCurrentWindow();

                if (motivoDialog.ShowDialog() == true)
                {
                    IsBusy = true;

                    var motivo = motivoDialog.Motivo;
                    var cancelarResult = await _citaService.CancelAsync(Cita.Id, motivo);

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

        private void Close()
        {
            var window = GetCurrentWindow();
            window?.Close();
        }

        private Window? GetCurrentWindow()
        {
            return Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
        }

        private bool CanCompletar()
        {
            return Cita.Estado == "Programada" || Cita.Estado == "En Proceso";
        }

        private bool CanCancelar()
        {
            return Cita.Estado != "Completada" && Cita.Estado != "Cancelada";
        }

        #endregion
    }
}