using System;
using System.Windows;

namespace SistemaControlAC.View
{
    /// <summary>
    /// Lógica de interacción para MotivoDialog.xaml
    /// </summary>
    public partial class MotivoDialog : Window
    {
        public string Motivo { get; private set; } = string.Empty;

        public MotivoDialog()
        {
            InitializeComponent();

            // Enfocar el TextBox al cargar
            Loaded += (s, e) => MotivoTextBox.Focus();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Validar que se haya ingresado un motivo
            if (string.IsNullOrWhiteSpace(MotivoTextBox.Text))
            {
                MessageBox.Show("Por favor ingrese el motivo de la cancelación.",
                              "Motivo Requerido",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                MotivoTextBox.Focus();
                return;
            }

            // Guardar el motivo y cerrar con resultado positivo
            Motivo = MotivoTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Cerrar sin guardar cambios
            DialogResult = false;
            Close();
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Permitir cerrar con Escape
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                DialogResult = false;
                Close();
            }
            // Permitir confirmar con Ctrl+Enter
            else if (e.Key == System.Windows.Input.Key.Enter &&
                     (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                ConfirmButton_Click(this, new RoutedEventArgs());
            }
        }
    }
}