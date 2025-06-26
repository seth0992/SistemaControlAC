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
}
