using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTProto.Client.Events
{
    public delegate Task AsyncEventHandler<TClient, TArg>(TClient client, TArg arg);
}
