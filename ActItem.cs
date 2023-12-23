using System;

namespace Suburb.ExpressRouter
{
    public class ActItem<T>
    {
        private readonly Action<T, Action<T>> action;
        private readonly Action abort;
        private readonly Action finallyCallback;
        
        public ActItem(
            Action<T, Action<T>> action, 
            Action finallyCallback = null, 
            Action abort = null)
        {
            this.action = action;
            this.finallyCallback = finallyCallback;
            this.abort = abort;
        }
        
        public void Invoke(T arg, Action<T> next) => action.Invoke(arg, next);

        public void Abort()
        {
            abort?.Invoke();
            finallyCallback?.Invoke();
        }
        
        public void Finally() => finallyCallback?.Invoke();
    }
}