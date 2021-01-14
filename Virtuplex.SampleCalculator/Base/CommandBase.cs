using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Virtuplex.SampleCalculator.Base
{
    /// <summary>
    /// Base class for commands.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        private Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public CommandBase(Func<bool> canExecute)
        {
            this._canExecute = canExecute ?? new Func<bool>(() => true);
        }

        /// <summary>
        /// Determines whether command can be executed.
        /// </summary>
        /// <param name="parameter">Parameter passed to command</param>
        /// <returns>True if can execute</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        /// <summary>
        /// Runs when command is executed.
        /// </summary>
        /// <param name="parameter">Parameter passed to command</param>
        public virtual void Execute(object parameter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invokes the <see cref="CanExecuteChanged"/> handler.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
