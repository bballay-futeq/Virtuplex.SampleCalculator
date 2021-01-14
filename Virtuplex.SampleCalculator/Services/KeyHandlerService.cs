using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Virtuplex.SampleCalculator.Calculations; 

namespace Virtuplex.SampleCalculator.Services
{
    /// <summary>
    /// Service used to avoid using 3rd party libs and breaking the MVVM pattern.
    /// </summary>
    public class KeyHandlerService
    {
        private Window _window;

        public event EventHandler<char> NumberKeyPressed;
        public event EventHandler<OperationType> ExpressionKeyPressed;
        public event EventHandler ClearPressed;
        public event EventHandler ClearEntryPressed;
        public event EventHandler EqualsPressed;
        public event EventHandler DelPressed;

        /// <summary>
        /// Attaches the service to a specific window.
        /// </summary>
        /// <param name="window">Window to attach the handlers for.</param>
        public void Attach(Window window)
        {
            _window = window;
            _window.KeyDown += HandleKeyDown;
        }

        public void Detach()
        {
            _window.KeyDown -= HandleKeyDown;
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            var keyString = new KeyConverter().ConvertToString(e.Key);
            var numberRegex = new Regex("^([0-9])|(NumPad[0-9])$");

            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                NumberKeyPressed?.Invoke(this, keyString.Last());
                return;
            } 

            if(e.Key == Key.Delete)
            {
                ClearEntryPressed?.Invoke(this, EventArgs.Empty);
                return;
            }

            switch(e.Key)
            {                
                case Key.Add: ExpressionKeyPressed?.Invoke(this, OperationType.Add);
                    break;
                case Key.Subtract:
                    ExpressionKeyPressed?.Invoke(this, OperationType.Subtract);
                    break;
                case Key.Multiply:
                    ExpressionKeyPressed?.Invoke(this, OperationType.Multiply);
                    break;
                case Key.Divide:
                    ExpressionKeyPressed?.Invoke(this, OperationType.Divide);
                    break;
                case Key.Escape:
                    ClearPressed?.Invoke(this, EventArgs.Empty);
                    break;
                case Key.Delete:
                    ClearEntryPressed?.Invoke(this, EventArgs.Empty);
                    break;
                case Key.Enter:
                    EqualsPressed?.Invoke(this, EventArgs.Empty);
                    break;
                case Key.Back:
                    DelPressed?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }
    }
}
