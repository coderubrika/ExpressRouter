using System;
using System.Collections.Generic;

namespace Suburb.ExpressRouter
{
    public class ActionSequence<T>
    {
        private readonly List<ActItem<T>> items = new();
        
        private int currentIndex;
        private ActionSequence<T> nextSequence;

        public int Count => items.Count;
        
        public IDisposable Add(ActItem<T> item)
        {
            items.Add(item);
            return new DisposableHook(() => items.Remove(item));
        }

        public void ConnectNext(ActionSequence<T> nextSequence)
        {
            this.nextSequence = nextSequence;
        }
        
        public void Call(T arg)
        {
            if (items.Count == 0)
                nextSequence?.Call(arg);
            
            items[0].Invoke(arg, Next);
        }

        private void Next(T arg)
        {
            currentIndex++;
            if (currentIndex >= items.Count)
            {
                currentIndex = 0;
                nextSequence?.Call(arg);
                return;
            }

            items[currentIndex].Invoke(arg, Next);
        }
    }
}