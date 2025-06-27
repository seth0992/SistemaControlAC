using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SistemaControlAC.View
{
    public partial class ClienteFormWindow : Window
    {
        public ClienteFormWindow()
        {
            InitializeComponent();

            // Configurar popups para que se cierren automáticamente
            Loaded += ClienteFormWindow_Loaded;
        }

        private void ClienteFormWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Encontrar todos los TextBox y configurar sus popups de validación
            ConfigureValidationPopups(this);
        }

        private void ConfigureValidationPopups(DependencyObject parent)
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is TextBox textBox)
                {
                    textBox.LostFocus += (s, e) => CloseValidationPopups();
                    textBox.TextChanged += (s, e) => CloseValidationPopups();
                }

                ConfigureValidationPopups(child);
            }
        }

        private void CloseValidationPopups()
        {
            // Cerrar todos los popups abiertos
            foreach (Popup popup in FindVisualChildren<Popup>(this))
            {
                if (popup.IsOpen && popup.Name.Contains("ValidationPopup"))
                {
                    popup.IsOpen = false;
                }
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}