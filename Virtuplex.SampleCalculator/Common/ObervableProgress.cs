using System;
using System.Reactive.Linq;
using System.Threading;

namespace Virtuplex.SampleCalculator.Common
{
    /// <summary>
    /// Helper methods for using observable <see cref="IProgress{T}"/> implementations. 
    /// Used to avoid lowering performance when reporting progress by spamming UI thread with updates.
    /// </summary>
    public static class ObservableProgress
    {
        /// <summary>
        /// Creates an observable and an <see cref="IProgress{T}"/> implementation that are linked together, suitable for UI consumption.
        /// When progress reports are sent to the <see cref="IProgress{T}"/> implementation, they are sampled according to <paramref name="sampleInterval"/> and then forwarded to the UI thread.
        /// This method must be called from the UI thread.
        /// </summary>
        /// <typeparam name="T">The type of progress reports.</typeparam>
        /// <param name="sampleInterval">How frequently progress reports are sent to the UI thread.</param>
        public static (IObservable<T> Observable, IProgress<T> Progress) CreateForUi<T>(TimeSpan? sampleInterval = null)
        {
            var (observable, progress) = Create<T>();
            observable = observable.Sample(sampleInterval ?? TimeSpan.FromMilliseconds(100))
                .ObserveOn(SynchronizationContext.Current);
            return (observable, progress);
        }

        /// <summary>
        /// Creates an observable and an <see cref="IProgress{T}"/> implementation that are linked together.
        /// When progress reports are sent to the <see cref="IProgress{T}"/> implementation, they are immediately and synchronously sent to the observable.
        /// </summary>
        /// <typeparam name="T">The type of progress reports.</typeparam>
        public static (IObservable<T> Observable, IProgress<T> Progress) Create<T>()
        {
            var progress = new EventProgress<T>();
            var observable = Observable.FromEvent<T>(handler => progress.OnReport += handler, handler => progress.OnReport -= handler);
            return (observable, progress);
        }

        private sealed class EventProgress<T> : IProgress<T>
        {
            public event Action<T> OnReport;
            void IProgress<T>.Report(T value) => OnReport?.Invoke(value);
        }
    }
}