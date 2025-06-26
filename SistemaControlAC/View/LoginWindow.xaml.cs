using SistemaControlAC.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SistemaControlAC.View
{
    /// <summary>
    /// Lógica de interacción para LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            // Suscribirse a cambios de propiedades si no funcionan los convertidores
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        // Manejar cambios de propiedades manualmente si los convertidores no funcionan
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                switch (e.PropertyName)
                {
                    case nameof(LoginViewModel.ErrorMessage):
                        // Mostrar/ocultar mensaje de error
                        if (ErrorMessageTextBlock != null)
                        {
                            ErrorMessageTextBlock.Visibility = string.IsNullOrWhiteSpace(viewModel.ErrorMessage)
                                ? Visibility.Collapsed
                                : Visibility.Visible;
                        }
                        break;

                    case nameof(LoginViewModel.IsBusy):
                        // Mostrar/ocultar barra de progreso y habilitar/deshabilitar botón
                        if (LoadingProgressBar != null)
                        {
                            LoadingProgressBar.Visibility = viewModel.IsBusy
                                ? Visibility.Visible
                                : Visibility.Collapsed;
                        }

                        if (LoginButton != null)
                        {
                            LoginButton.IsEnabled = !viewModel.IsBusy;
                        }
                        break;
                }
            }
        }

        // Limpiar suscripciones al cerrar
        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.OnClosed(e);
        }
    }
}
