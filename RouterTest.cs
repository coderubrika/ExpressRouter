using System;
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
            //Test5();
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

        // private void Test2()
        // {
        //     CreateEndpoints(10);
        //
        //     routerService.Use((from, to) => Debug.Log($"{from?.Name}->{to?.Name}")).Dispose();
        //     routerService.Use((from, to) => Debug.Log($"{from?.Name}!!->{to?.Name}!!")).Dispose();
        //     routerService.Use((from, to) => Debug.Log($"{from?.Name}**->{to?.Name}**"), "9", "1").Dispose();
        //     routerService.Use((from, to) => Debug.Log($"{from?.Name}++->{to?.Name}++"), null, "9").Dispose();
        //     routerService.Use((from, to) => Debug.Log($"{from?.Name}&&->{to?.Name}&&"), "4");
        //
        //     routerService.GoTo("4");
        //     routerService.GoTo("9");
        //     routerService.GoTo("1");
        //     routerService.GoTo("9");
        //     routerService.GoTo("1");
        // }

        // private void Test3()
        // {
        //     var tree = new ActionSequence();
        //     
        //     tree.Add(next =>
        //     {
        //         this.Log("1a");
        //         next.Invoke();
        //     });
        //     
        //     tree.Add(next =>
        //     {
        //         this.Log("2a");
        //         next.Invoke();
        //     });
        //     
        //     tree.Add(next =>
        //     {
        //         this.Log("3a");
        //         next.Invoke();
        //     });
        //     
        //     tree.Add(next =>
        //     {
        //         this.Log("4a");
        //         next.Invoke();
        //     });
        //     
        //     tree.Call();
        // }
        //
        // private void Test4()
        // {
        //     var node = new ActionSequence();
        //
        //     for (int i = 0; i < 10; i++)
        //     {
        //         int idx = i;
        //         node.Add(next =>
        //         {
        //             this.Log($"{idx}a");
        //             Observable.Timer(TimeSpan.FromSeconds(1))
        //                 .Subscribe(_ => next.Invoke());
        //         });
        //     }
        //     
        //     node.Call();
        // }
        //
        // private void Test5()
        // {
        //     ActionSequence firstSeq = null;
        //     ActionSequence prevSeq = null;
        //     
        //     for (int j = 0; j < 10; j++)
        //     {
        //         var sequence = new ActionSequence();
        //         if (j == 0)
        //             firstSeq = sequence;
        //         
        //         for (int i = 0; i < 10; i++)
        //         {
        //             int jdx = j;
        //             int idx = i;
        //             sequence.Add(next =>
        //             {
        //                 this.Log($"{jdx}/{idx}");
        //                 Observable.Timer(TimeSpan.FromSeconds(0.5f))
        //                     .Subscribe(_ => next.Invoke());
        //             });
        //         }
        //         
        //         prevSeq?.ConnectNext(sequence);
        //         prevSeq = sequence;
        //     }
        //     
        //     firstSeq.Call();
        // }
    }
}