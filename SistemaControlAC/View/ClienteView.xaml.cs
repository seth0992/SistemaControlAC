using SistemaControlAC.ViewModel;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SistemaControlAC.View
{
    /// <summary>
    /// Lógica de interacción para ClienteView.xaml
    /// </summary>
    public partial class ClienteView : UserControl
    {
        public ClienteView()
        {
            InitializeComponent();
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ClienteViewModel viewModel && viewModel.SelectedCliente != null)
            {
                // Ejecutar comando para ver detalles del cliente
                if (viewModel.ViewClienteCommand.CanExecute(null))
                {
                    viewModel.ViewClienteCommand.Execute(null);
                }
            }
        }
    }
    // Convertidor para mostrar mensaje cuando no hay elementos
    public class CountToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public static CountToVisibilityConverter Instance { get; } = new CountToVisibilityConverter();

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
