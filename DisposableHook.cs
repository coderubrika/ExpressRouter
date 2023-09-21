using System;

namespace Suburb.ExpressRouter
{
    internal class DisposableHook : IDisposable
    {
        private readonly Action onDispose;

        public DisposableHook(Action onDispose)
        {
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            onDispose?.Invoke();
        }
    }
}
