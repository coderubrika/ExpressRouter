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
        private readonly Dictionary<string, Action<IEndpoint, IEndpoint>> middlewares = new();

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

        public IDisposable Use(Action<IEndpoint, IEndpoint> middleware, string nameFrom = null, string nameTo = null)
        {
            (nameFrom, nameTo) = TransformNames(nameFrom, nameTo);

            string key = $"{nameFrom}->{nameTo}";

            if (middlewares.ContainsKey(key))
                middlewares[key] += middleware;
            else
                middlewares.Add($"{nameFrom}->{nameTo}", middleware);

            return new DisposableHook(() => middlewares[key] -= middleware);
        }

        private void ApplyMiddlewares(IEndpoint from = null, IEndpoint to = null)
        {
            string nameFrom, nameTo, filter;
            (nameFrom, nameTo) = TransformNames(from?.Name, to?.Name);

            if (middlewares.TryGetValue(ALL_FILTER, out Action<IEndpoint, IEndpoint> middleware))
                middlewares[ALL_FILTER]?.Invoke(from, to);

            filter = $"{nameFrom}->*";
            if (nameFrom != ALL && middlewares.TryGetValue(filter, out middleware))
                middlewares[filter]?.Invoke(from, to);

            filter = $"*->{nameTo}";
            if (nameTo != ALL && middlewares.TryGetValue(filter, out middleware))
                middlewares[filter]?.Invoke(from, to);

            filter = $"{nameFrom}->{nameTo}";
            if (nameFrom != ALL && nameTo != ALL && middlewares.TryGetValue(filter, out middleware))
                middlewares[filter]?.Invoke(from, to);
        }

        private (string NameFrom, string NameTo) TransformNames(string nameFrom, string nameTo)
        {
            if (string.IsNullOrEmpty(nameFrom))
                nameFrom = ALL;

            if (string.IsNullOrEmpty(nameTo))
                nameTo = ALL;

            return new ValueTuple<string, string>(nameFrom, nameTo);
        }
    }
}
