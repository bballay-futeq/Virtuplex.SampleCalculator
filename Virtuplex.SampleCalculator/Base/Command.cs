using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Virtuplex.SampleCalculator.Base
{
    /// <summary>
    /// Basic command implementation.
    /// </summary>
    public class Command : CommandBase
    {
        private Action _execute;

        /// <summary>
        /// Creates a non-generic <see cref="Command"/> instance.
        /// </summary>
        /// <param name="execute">Action to invoke when command is executed.</param>
        /// <param name="canExecute">Determines whether the command can be executed.</param>
        public Command(Action execute, Func<bool> canExecute = null): base(canExecute)
        {
            if(execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            this._execute = execute; 
        } 

        
        public override void Execute(object parameter)
        {
            _execute(); 
        }
    }

    /// <summary>
    /// Generic implementation of command.
    /// </summary>
    /// <typeparam name="T">Type to which the parameter will be converted to.</typeparam>
    public class Command<T> : CommandBase
    {
        private Action<T> _execute;

        /// <summary>
        /// Creates a generic <see cref="Command"/> instance with type parameter.
        /// </summary>
        /// <param name="execute">Action to invoke when command is executed.</param>
        /// <param name="canExecute">Determines whether the command can be executed.</param>
        public Command(Action<T> execute, Func<bool> canExecute = null): base(canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            this._execute = execute; 
        }

        /// <summary>
        /// Runs when command is executed.
        /// </summary>
        /// <param name="parameter">Parameter passed to command</param>
        public override void Execute(object parameter)
        {
            _execute((T)Convert.ChangeType(parameter, typeof(T)));
        }
    }
}
