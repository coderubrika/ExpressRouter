using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;


namespace Suburb.ExpressRouter
{
    public class Router
    {
        private readonly Stack<IEndpoint> history = new();
        private readonly Dictionary<string, IEndpoint> endpoints = new();

        private readonly Dictionary<MiddlewareOrder, HashSet<(Rule rule, ActItem<FromTo> middleware)>> middlewares = new()
        {
            { MiddlewareOrder.From, new() },
            { MiddlewareOrder.Middle, new() },
            { MiddlewareOrder.To, new() }
        };


        private readonly ActionSequence<FromTo> sequence = new();
        
        public bool GoTo(string name)
        {
            if (!endpoints.TryGetValue(name, out IEndpoint to))
                return false;

            if (history.TryPeek(out IEndpoint from) && from == to)
                return false;

            history.Push(to);
            ApplyMiddlewares(new FromTo
            {
                From = from,
                To = to
            });
            return true;
        }

        public bool GoToPrevious()
        {
            if (history.Count < 2)
                return false;

            IEndpoint from = history.Pop();
            IEndpoint to = history.Peek();
            ApplyMiddlewares(new FromTo
            {
                From = from,
                To = to
            });
            return true;
        }

        public bool GoToPrevious(string name)
        {
            if (history.Count < 2)
                return false;

            if (!endpoints.TryGetValue(name, out IEndpoint to))
                return false;

            if (history.Peek() == to)
                return false;

            if (!history.Contains(to))
            {
                history.Clear();
                history.Push(to);
                return true;
            }

            IEndpoint from = history.Pop();

            while (history.TryPop(out IEndpoint target))
            {
                if (target == to)
                {
                    history.Push(to);
                    break;
                }
            }

            ApplyMiddlewares(new FromTo
            {
                From = from,
                To = to
            });
            return true;
        }

        public IEndpoint[] GetPathToPrevious(string name, bool isExcludeFrom = false)
        {
            if (history.Count < 2)
                return Array.Empty<IEndpoint>();

            if (!endpoints.TryGetValue(name, out IEndpoint to))
                return Array.Empty<IEndpoint>();

            if (history.Peek() == to)
                return Array.Empty<IEndpoint>();

            if (!history.Contains(to))
                return history.ToArray();

            IEnumerable<IEndpoint> path = isExcludeFrom ? history.Skip(1) : history;

            return path
                .TakeWhile(x => x != to)
                .ToArray();
        }

        public void AddEndpoint(IEndpoint endpoint)
        {
            endpoints[endpoint.Name] = endpoint;
        }

        public bool ContainsEndpoint(string name)
        {
            return endpoints.ContainsKey(name);
        }
        
        public IEnumerable<string> GetHistory()
        {
            return history
                .Select(endpoint => endpoint.Name)
                .Reverse();
        }

        public IEndpoint GetLast()
        {
            return history.TryPeek(out IEndpoint endpoint) ? endpoint : null;
        }

        public IDisposable Use(
            ActItem<FromTo> middleware, Rule rule,
            MiddlewareOrder order = MiddlewareOrder.Middle)
        {
            ValueTuple<Rule, ActItem<FromTo>> item = new(rule, middleware);
            middlewares[order].Add(item);
            return Disposable.Create(() => middlewares[order].Remove(item));
        }

        private void ApplyMiddlewares(FromTo points)
        {
            sequence.Clear();
            string nameFrom = points.From?.Name;
            string nameTo = points.To?.Name;

            BindByOrder(MiddlewareOrder.From, nameFrom, nameTo, sequence);
            BindByOrder(MiddlewareOrder.Middle, nameFrom, nameTo, sequence);
            BindByOrder(MiddlewareOrder.To, nameFrom, nameTo, sequence);
            
            sequence.Call(points);
        }

        private void BindByOrder(
            MiddlewareOrder order, 
            string nameFrom, 
            string nameTo, 
            ActionSequence<FromTo> sequence)
        {
            var middlewaresList = middlewares[order]
                .Where(item => item.rule.Match(nameFrom, nameTo))
                .Select(item => item.middleware);

            foreach (var middleware in middlewaresList)
                sequence.Add(middleware);
        }
    }
}
