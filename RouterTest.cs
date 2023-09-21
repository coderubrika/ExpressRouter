using System.Linq;
using UnityEngine;

namespace Suburb.ExpressRouter
{
    public class Endpoint : IEndpoint
    {
        public string Name { get; }

        public Endpoint(string name)
        {
            Name = name;
        }
    }

    public class RouterTest : MonoBehaviour
    {
        private readonly Router routerService = new Router();

        private void Awake()
        {
            Test2();
        }

        private void AddAndGoTo(string name)
        {
            if (!routerService.ContainsEndpoint(name))
                routerService.AddEndpoint(new Endpoint(name));

            routerService.GoTo(name);
            Debug.Log(string.Join('/', routerService.GetHistory()));
        }

        private void AddAndGoTo(int count)
        {
            for (int i = 0; i < count; i++)
                AddAndGoTo(i.ToString());
        }

        private void GoBack(int count)
        {
            for (int i = 0; i < count; i++)
            {
                routerService.GoToPrevious();
                Debug.Log(string.Join('/', routerService.GetHistory()));
            }
        }

        private void GoBackTo(string name)
        {
            routerService.GoToPrevious(name);
            Debug.Log(string.Join('/', routerService.GetHistory()));
        }

        private void CreateEndpoints(int count)
        {
            for (int i = 0; i < count; i++)
                routerService.AddEndpoint(new Endpoint(i.ToString()));
        }

        private void Test1()
        {
            AddAndGoTo(5);
            if (!routerService.ContainsEndpoint("b"))
                routerService.AddEndpoint(new Endpoint("b"));
            Debug.Log(string.Join("/", routerService.GetPathToPrevious("2").Select(p => p.Name).ToArray()));
            Debug.Log(string.Join("/", routerService.GetPathToPrevious("b").Select(p => p.Name).ToArray()));
            AddAndGoTo("4");
            GoBack(5);
            AddAndGoTo(5);
            GoBackTo("2");
            GoBackTo("3");
            GoBackTo("3");
            GoBackTo("f");
        }

        private void Test2()
        {
            CreateEndpoints(10);

            routerService.Use((from, to) => Debug.Log($"{from?.Name}->{to?.Name}")).Dispose();
            routerService.Use((from, to) => Debug.Log($"{from?.Name}!!->{to?.Name}!!")).Dispose();
            routerService.Use((from, to) => Debug.Log($"{from?.Name}**->{to?.Name}**"), "9", "1").Dispose();
            routerService.Use((from, to) => Debug.Log($"{from?.Name}++->{to?.Name}++"), null, "9").Dispose();
            routerService.Use((from, to) => Debug.Log($"{from?.Name}&&->{to?.Name}&&"), "4");

            routerService.GoTo("4");
            routerService.GoTo("9");
            routerService.GoTo("1");
            routerService.GoTo("9");
            routerService.GoTo("1");
        }
    }
}