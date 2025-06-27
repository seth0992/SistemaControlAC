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
using System.Windows.Shapes;

namespace SistemaControlAC.View
{
    /// <summary>
    /// Lógica de interacción para CitaFormWindow.xaml
    /// </summary>
    public partial class CitaFormWindow : Window
    {
        public CitaFormWindow()
        {
            InitializeComponent();
        }
        private void ClienteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ViewModel.CitaFormViewModel viewModel)
            {
                viewModel.ClienteChangedCommand?.Execute(null);
            }
        }

        private void TipoServicioComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ViewModel.CitaFormViewModel viewModel)
            {
                viewModel.CalcularDuracionCommand?.Execute(null);
            }
        }
    }
}
