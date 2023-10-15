using System;

namespace Suburb.ExpressRouter
{
    public enum MiddlewareOrder
    {
        From,
        To,
        Middle
    }
    
    public class OrderedHost
    {
        private readonly ActionSequence<FromTo> fromMiddlewares = new();
        private readonly ActionSequence<FromTo> middleMiddlewares = new();
        private readonly ActionSequence<FromTo> toMiddlewares = new();
        
        public IDisposable AddMiddleware(
            MiddlewareOrder order, 
            ActItem<FromTo> middleware)
        {
            return order switch
            {
                MiddlewareOrder.From => fromMiddlewares.Add(middleware),
                MiddlewareOrder.To => toMiddlewares.Add(middleware),
                _ => middleMiddlewares.Add(middleware)
            };
        }

        public ActionSequence<FromTo> GetSequence(MiddlewareOrder order)
        {
            return order switch
            {
                MiddlewareOrder.From => fromMiddlewares.Count > 0 ? fromMiddlewares : null,
                MiddlewareOrder.Middle => middleMiddlewares.Count > 0 ? middleMiddlewares : null,
                _ => toMiddlewares.Count > 0 ? toMiddlewares : null
            };
        }
    }
}