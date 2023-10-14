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
        private readonly ActionSequence fromMiddlewares = new();
        private readonly ActionSequence middleMiddlewares = new();
        private readonly ActionSequence toMiddlewares = new();
        
        public IDisposable AddMiddleware(
            MiddlewareOrder order, 
            Action<IEndpoint, IEndpoint, Action> middleware, 
            IEndpoint from = null, 
            IEndpoint to = null)
        {
            return order switch
            {
                MiddlewareOrder.From => fromMiddlewares.Add(next => middleware.Invoke(from, to, next)),
                MiddlewareOrder.To => toMiddlewares.Add(next => middleware.Invoke(from, to, next)),
                _ => middleMiddlewares.Add(next => middleware.Invoke(from, to, next))
            };
        }

        public ActionSequence GetSequence(MiddlewareOrder order)
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