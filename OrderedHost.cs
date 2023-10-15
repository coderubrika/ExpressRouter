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
        private readonly ActionSequence<(IEndpoint From, IEndpoint To)> fromMiddlewares = new();
        private readonly ActionSequence<(IEndpoint From, IEndpoint To)> middleMiddlewares = new();
        private readonly ActionSequence<(IEndpoint From, IEndpoint To)> toMiddlewares = new();
        
        public IDisposable AddMiddleware(
            MiddlewareOrder order, 
            Action<(IEndpoint From, IEndpoint To), Action<(IEndpoint From, IEndpoint To)>> middleware)
        {
            return order switch
            {
                MiddlewareOrder.From => fromMiddlewares.Add(middleware),
                MiddlewareOrder.To => toMiddlewares.Add(middleware),
                _ => middleMiddlewares.Add(middleware)
            };
        }

        public ActionSequence<(IEndpoint From, IEndpoint To)> GetSequence(MiddlewareOrder order)
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