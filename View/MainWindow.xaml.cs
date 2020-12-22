using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BrightnessChanger
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            App.RegisterWindow(this);
            this.DataContext = new MainWindowViewModel(this);
        }


        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            KeyboardManager.DisableSystemKeys(this.VisualTreeChildren<IHandleKeyboardHookControl>().ToList());
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            KeyboardManager.EnableSystemKeys();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    break;
                case WindowState.Minimized:
                    this.Hide();
                    break;
                case WindowState.Normal:
                    break;
            }
        }
    }
}
