using System;
using System.Windows.Input;

namespace Main.Models
{
    public class RelayCommand : ICommand
    {
        private Action<object> _executeHandler;
        private Func<object, bool> _canExecuteHandler;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> executeHandler, Func<object, bool> canExecuteHandler)
        {
            _executeHandler = executeHandler ?? throw new ArgumentNullException("execute handler can not be null");
            _canExecuteHandler = canExecuteHandler ?? throw new ArgumentNullException("canExecute handler can not be null");
        }

        public RelayCommand(Action<object> execute) : this(execute, (x) => true)
        { }

        public bool CanExecute(object parameter)
        {
            return _canExecuteHandler(parameter);
        }

        public void Execute(object parameter)
        {
            _executeHandler(parameter);
        }

    }
}
