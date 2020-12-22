using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BrightnessChanger
{
    public class MainWindowViewModel : NotifyPropertyChanged
    {

        public KeyboardShortcut BrightUpShortcut
        {
            get
            {
                return this.Config.BrightUpShortcut;
            }
            set
            {
                this.Config.BrightUpShortcut = value;
                this.RaisePropertyChanged();
            }
        }

        public KeyboardShortcut BrightDownShortcut
        {
            get
            {
                return this.Config.BrightDownShortcut;
            }
            set
            {
                this.Config.BrightDownShortcut = value;
                this.RaisePropertyChanged();
            }
        }


        private Config Config;
        private readonly MainWindow Window;
        public MainWindowViewModel(MainWindow w)
        {
            this.Config = new Config();
            this.Window = w;
            this.SaveCommand = new RelayCommand(this.Save);
            this.OpenConfigurationCommand = new RelayCommand(this.OpenConfiguration);
            this.CloseAppCommand = new RelayCommand(this.CloseApp);
            if(this.Config.BrightUpShortcut != null || this.Config.BrightDownShortcut!= null )
            {
                w.WindowState = WindowState.Minimized;
            }
        }

        
        public RelayCommand OpenConfigurationCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand CloseAppCommand { get; set; }

        private void Save()
        {
            this.Config.Save();
        }

        private void OpenConfiguration()
        {
            if (this.Window.IsVisible)
            {
                this.Window.Activate();
            }
            else
            {
                this.Window.Show();
                this.Window.WindowState = WindowState.Normal;
                this.Window.Activate();
            }
        }

        private void CloseApp()
        {
            App.Current.Shutdown();
        }
    }
}
