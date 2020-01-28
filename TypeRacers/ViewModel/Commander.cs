using System;
using System.Windows.Input;

namespace TypeRacers.ViewModel
{
    public class CommandHandler : ICommand
    {
        private readonly Action toExecute;
        private readonly Func<bool> canExecute;

        public CommandHandler(Action action, Func<bool> canExecute)
        {
            toExecute = action;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return canExecute.Invoke();
        }

        public void Execute(object parameter)
        {
            toExecute();
        }
    }
}