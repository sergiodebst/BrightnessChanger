using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrightnessChanger
{
    public class RelayCommand : ICommand
    {


        private readonly Action<object> _Execute;
        private readonly Func<object, bool> _CanExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute)
        {
            _Execute = execute;
        }
        public RelayCommand(Action execute) : this((obj) => execute()) { }
        
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute) : this(execute)
        {
            _CanExecute = canExecute;
        }



        public bool CanExecute(object parameter)
        {
            return _CanExecute == null || _CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _Execute(parameter);
        }
    }

    public class RelayCommand<T>:RelayCommand
    {
        public RelayCommand(Action<T> execute) : base((obj) => execute((T)obj)) { }
    }
}
