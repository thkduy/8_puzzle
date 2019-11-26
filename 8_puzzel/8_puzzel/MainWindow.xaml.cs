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

namespace _8_puzzel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Load clicked");
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Help clicked");
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button New clicked");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Save clicked");
        }

        private void BtnChooseImg_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Choose clicked");
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Pause clicked");
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }

        private void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Left clicked");
        }

        private void BtnRight_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Right clicked");
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Down clicked");
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Up clicked");
        }
    }
}
