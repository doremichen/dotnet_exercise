/**
 * Description: Thi class is RelayCommand , which implements ICommand interface.
 * 
 * Author: Adam chen
 * Date: 2025/08/04
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TimetableGenerator.ViewModels.Commands
{
    public class RelayCommand : ICommand
    {
        // Action _execute: readonly
        private readonly Action _execute;
        // Func<bool> _canExecute: readonly
        private readonly Func<bool> _canExecute;

        // Constructor to initialize the command with execute and canExecute actions
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (() => true);
        }


        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();
        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
