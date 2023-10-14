using System;
using System.Collections.Generic;
using System.Linq;


namespace Suburb.ExpressRouter
{
    public class Router
    {
        private const string ALL = "*";
        private const string ALL_FILTER = "*->*";

        private readonly Stack<IEndpoint> history = new();
        private readonly Dictionary<string, IEndpoint> endpoints = new();
        private readonly Dictionary<string, OrderedHost> middlewares = new();
        
        public bool GoTo(string name)
        {
            if (!endpoints.TryGetValue(name, out IEndpoint to))
                return false;

            if (history.TryPeek(out IEndpoint from) && from == to)
                return false;

            history.Push(to);
            ApplyMiddlewares(from, to);
            return true;
        }

        public bool GoToPrevious()
        {
            if (history.Count < 2)
                return false;

            IEndpoint from = history.Pop();
            IEndpoint to = history.Peek();
            ApplyMiddlewares(from, to);
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

            ApplyMiddlewares(from, to);
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
            Action<IEndpoint, IEndpoint, Action> middleware,
            string nameFrom = null, 
            string nameTo = null,
            MiddlewareOrder order = MiddlewareOrder.Middle)
        {
            (nameFrom, nameTo) = TransformNames(nameFrom, nameTo);
            
            string key = $"{nameFrom}->{nameTo}";
            IDisposable disposable;
            if (middlewares.TryGetValue(key, out var orderedMiddleware))
                disposable = orderedMiddleware.AddMiddleware(order, middleware);
            else
            {
                var host = new OrderedHost();
                disposable = host.AddMiddleware(order, middleware);
                middlewares.Add(key, host);
            }

            return disposable;
        }

        private void ApplyMiddlewares(IEndpoint from = null, IEndpoint to = null)
        {
            string nameFrom, nameTo;
            (nameFrom, nameTo) = TransformNames(from?.Name, to?.Name);

            ActionSequence sequence;
            Action call;
            // порядок неправильный
            (sequence, call) = BindByOrder(MiddlewareOrder.From, nameFrom, nameTo, null, null);
            (sequence, call) = BindByOrder(MiddlewareOrder.Middle, nameFrom, nameTo, sequence, call);
            (sequence, call) = BindByOrder(MiddlewareOrder.To, nameFrom, nameTo, sequence, call);
            
            call?.Invoke();
        }

        private (ActionSequence Tail, Action StartCall) BindSequence(
            ActionSequence previous, 
            Action startCall, 
            ActionSequence next)
        {
            if (next == null)
                return (previous, startCall);
            
            if (previous != null)
                previous.ConnectNext(next);
            else
                startCall = next.Call;

            return (next, startCall);
        }

        private (ActionSequence Tail, Action StartCall) BindByOrder(
            MiddlewareOrder order, 
            string nameFrom, 
            string nameTo, 
            ActionSequence startSequence, 
            Action startCall)
        {
            if (middlewares.TryGetValue(ALL_FILTER, out OrderedHost host))
                (startSequence, startCall) = BindSequence(startSequence, startCall, host.GetSequence(order));
            
            var filter = $"{nameFrom}->*";
            if (nameFrom != ALL && middlewares.TryGetValue(filter, out host))
                (startSequence, startCall) = BindSequence(startSequence, startCall, host.GetSequence(order));
            
            filter = $"*->{nameTo}";
            if (nameTo != ALL && middlewares.TryGetValue(filter, out host))
                (startSequence, startCall) = BindSequence(startSequence, startCall, host.GetSequence(order));
            
            filter = $"{nameFrom}->{nameTo}";
            if (nameFrom != ALL && nameTo != ALL && middlewares.TryGetValue(filter, out host))
                (startSequence, startCall) = BindSequence(startSequence, startCall, host.GetSequence(order));

            return (startSequence, startCall);
        }
        
        private (string NameFrom, string NameTo) TransformNames(string nameFrom, string nameTo)
        {
            if (string.IsNullOrEmpty(nameFrom))
                nameFrom = ALL;

            if (string.IsNullOrEmpty(nameTo))
                nameTo = ALL;

            return (nameFrom, nameTo);
        }
    }
}
