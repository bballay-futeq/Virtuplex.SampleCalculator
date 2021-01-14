using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Virtuplex.SampleCalculator.Base
{
    /// <summary>
    /// Base class for main view model.
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Simple method for changing the value and invoking the PropertyChagned event handler.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">Backing field</param>
        /// <param name="newValue">Value to set</param>
        /// <returns></returns>
        protected bool Set<T>(
            ref T field,
            T newValue = default(T),
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;

            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Invokes the PropertyChanged event handler.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)
            {

            });
        }
    }
}
