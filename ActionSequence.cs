using System;
using System.Collections.Generic;

namespace Suburb.ExpressRouter
{
    public class ActionSequence
    {
        private readonly List<Action<Action>> actions = new();
        
        private int currentIndex;
        private ActionSequence nextSequence;
        
        public IDisposable Add(Action<Action> action)
        {
            actions.Add(action);
            return new DisposableHook(() => actions.Remove(action));
        }

        public void ConnectNext(ActionSequence nextSequence)
        {
            this.nextSequence = nextSequence;
        }
        
        public void Call()
        {
            if (actions.Count == 0)
                return;
            
            actions[0].Invoke(Next);
        }

        private void Next()
        {
            currentIndex++;
            if (currentIndex >= actions.Count)
            {
                currentIndex = 0;
                nextSequence?.Call();
                return;
            }

            actions[currentIndex].Invoke(Next);
        }
    }
}