using System;

namespace Suburb.ExpressRouter
{
    public class ActItem<T>
    {
        private readonly Action<T, Action<T>> action;
        public ActItem(Action<T, Action<T>> action) => this.action = action;
        public void Invoke(T arg, Action<T> next) => action.Invoke(arg, next);
    }
}