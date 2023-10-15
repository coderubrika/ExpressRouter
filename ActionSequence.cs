using System;
using System.Collections.Generic;

namespace Suburb.ExpressRouter
{
    public class ActionSequence<T>
    {
        private readonly List<Action<T, Action<T>>> actions = new();
        
        private int currentIndex;
        private ActionSequence<T> nextSequence;

        public int Count => actions.Count;
        
        public IDisposable Add(Action<T, Action<T>> action)
        {
            actions.Add(action);
            return new DisposableHook(() => actions.Remove(action));
        }

        public void ConnectNext(ActionSequence<T> nextSequence)
        {
            this.nextSequence = nextSequence;
        }
        
        public void Call(T arg)
        {
            if (actions.Count == 0)
                nextSequence?.Call(arg);
            
            actions[0].Invoke(arg, Next);
        }

        private void Next(T arg)
        {
            currentIndex++;
            if (currentIndex >= actions.Count)
            {
                currentIndex = 0;
                nextSequence?.Call(arg);
                return;
            }

            actions[currentIndex].Invoke(arg, Next);
        }
    }
}