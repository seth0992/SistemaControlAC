using SistemaControlAC.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            // Suscribirse a cambios de propiedades del ViewModel
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged!;
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

        // Manejar cambios de propiedades manualmente para compatibilidad
        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                switch (e.PropertyName)
                {
                    case nameof(LoginViewModel.ErrorMessage):
                        // El binding automático maneja la visibilidad del error
                        // Solo necesitamos asegurar que el binding funcione correctamente
                        break;

                    case nameof(LoginViewModel.IsBusy):
                        // Habilitar/deshabilitar botón y mostrar/ocultar indicador de carga
                        if (LoginButton != null)
                        {
                            LoginButton.IsEnabled = !viewModel.IsBusy;
                        }
                        // El binding automático maneja la visibilidad del ProgressBar
                        break;
                }
            }
        }

        // Limpiar suscripciones al cerrar
        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged!;
            }
            base.OnClosed(e);
        }
    }
}