using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace WpfFolderBrowser
{
    /// <summary>
    /// NameInputWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class NameInputWindow : Window
    {
        private bool result = false;
        private string _NameText;
        public string NameText { get => _NameText; }

        public NameInputWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileNameCheck())
            {
                result = true;
                _NameText = inputBox.Text;
                Close();
            }
        }

        public new bool? ShowDialog()
        {
            base.ShowDialog();
            return result;
        }

        private bool FileNameCheck()
        {
            return !Regex.IsMatch(inputBox.Text, "[\\/:*?\"<>|]");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            inputBox.Focus();
            if (Owner != null)
            {
                Left = Owner.Left + (Owner.Width / 2 - Width / 2);
                Top = Owner.Top + (Owner.Height / 2 - Height / 2);
            }
        }
    }
}
