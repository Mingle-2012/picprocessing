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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Page_Navigation_App.Windows
{
    /// <summary>
    /// PopWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PopWindow : Window
    {
        public object parameter1 { get; set; }
        public object parameter2 { get; set; }
        public object parameter3 { get; set; }
        public object parameter4 { get; set; }

        public PopWindow(string paramName1, 
                         string hintt1,
                         string paramName2 = null, 
                         string hintt2 = null,
                         string paramName3 = null, 
                         string hintt3 = null,
                         string paramName4 = null,
                         string hintt4 = null)
                    
        {
            InitializeComponent();
            param1.Content = paramName1;
            hint1.Content = hintt1;
            if(string.IsNullOrEmpty(paramName2))
            {
                value2.Visibility = Visibility.Collapsed;
                param2.Visibility = Visibility.Collapsed;
                hint2.Visibility = Visibility.Collapsed;
            }
            else
            {
                param2.Content = paramName2;
                hint2.Content = hintt2;

            }
            if (string.IsNullOrEmpty(paramName3))
            {
                value3.Visibility = Visibility.Collapsed;
                param3.Visibility = Visibility.Collapsed;
                hint3.Visibility = Visibility.Collapsed;
            }
            else
            {
                param3.Content = paramName3;
                hint3.Content = hintt3;
            }
            if (string.IsNullOrEmpty(paramName4))
            {
                value4.Visibility = Visibility.Collapsed;
                param4.Visibility = Visibility.Collapsed;
                hint4.Visibility = Visibility.Collapsed;
            }
            else
            {
                param4.Content = paramName4;
                hint4.Content = hintt4;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e) {
            parameter1 = value1.Text;
            if (param2.Visibility is not Visibility.Collapsed)
            {
                parameter2 = value2.Text;
            }
            if (param3.Visibility is not Visibility.Collapsed)
            {
                parameter3 = value3.Text;
            }
            if (param4.Visibility is not Visibility.Collapsed)
            {
                parameter4 = value4.Text;
            }
            DialogResult = true;
            Close();
        }
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Submit_Click(sender, e);
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
