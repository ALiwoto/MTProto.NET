using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MTProto.Client.Events
{
    public class EventManager<TClient, TArg> where TClient : class
    {
        public List<AsyncEventHandler<TClient, TArg>> EventHandlers { get; set; }

        public EventManager(List<AsyncEventHandler<TClient, TArg>> handlers)
        {
            EventHandlers = handlers;
        }

        public async Task InvokeHandlers(TClient client, TArg arg)
        {

            foreach (var handler in EventHandlers)
            {
                try
                {
                    await handler?.Invoke(client, arg);
                }
                catch (ContinueGroupsException)
                {
                    return;
                }
            }
        }

        public static EventManager<TClient, TArg> operator +(EventManager<TClient, TArg> manager, AsyncEventHandler<TClient, TArg> handler)
        {
            manager ??= new EventManager<TClient, TArg>(new List<AsyncEventHandler<TClient, TArg>>());
            manager.EventHandlers.Add(handler);
            return manager;
        }
    }
}
