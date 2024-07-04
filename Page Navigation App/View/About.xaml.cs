using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Security.Policy;

namespace Page_Navigation_App.View
{
    /// <summary>
    /// Interaction logic for Customers.xaml
    /// </summary>
    public partial class About : UserControl
    {
        public About()
        {
            InitializeComponent();
        }

        private void OpenUrl(string url)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开链接: {ex.Message}");
            }
        }

        private void Click_button(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://github.com/Mingle-2012/picprocessing");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://github.com/Mingle-2012");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://github.com/sirius-1024");
        }
    }
}
