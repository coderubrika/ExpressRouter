using System;
using System.Collections.Generic;
using UniRx;

namespace Suburb.ExpressRouter
{
    public class ActionSequence<T>
    {
        private readonly List<ActItem<T>> items = new();
        
        private int currentIndex;
        private ActionSequence<T> nextSequence;
        private bool inProcess;
        
        public int Count => items.Count;
        
        public IDisposable Add(ActItem<T> item)
        {
            items.Add(item);
            return Disposable.Create(() => items.Remove(item));
        }

        public void ConnectNext(ActionSequence<T> nextSequence)
        {
            this.nextSequence = nextSequence;
        }
        
        public bool Call(T arg)
        {
            if (inProcess)
                return false;

            if (items.Count == 0)
                return nextSequence != null && nextSequence.Call(arg);

            inProcess = true;
            items[0].Invoke(arg, Next);
            return true;
        }

        public void Disassemble()
        {
            nextSequence?.Disassemble();
            nextSequence = null;
        }
        
        public void Abort()
        {
            if (!inProcess)
            {
                nextSequence?.Abort();
                return;
            }

            inProcess = false;

            items[currentIndex].Abort();
            for (int i = currentIndex + 1; i < items.Count; i++)
                items[currentIndex].Finally();

            nextSequence?.Finally();
        }

        public void Clear()
        {
            Abort();
            Disassemble();
            currentIndex = 0;
            items.Clear();
        }
        
        private void Finally()
        {
            if (inProcess)
                Abort();

            foreach (var item in items)
                item.Finally();
            
            nextSequence?.Finally();
        }
        
        private void Next(T arg)
        {
            items[currentIndex].Finally();
            currentIndex++;
            if (currentIndex >= items.Count)
            {
                currentIndex = 0;
                nextSequence?.Call(arg);
                inProcess = false;
                return;
            }

            items[currentIndex].Invoke(arg, Next);
        }
    }
}