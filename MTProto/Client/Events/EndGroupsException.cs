using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTProto.Client.Events
{
    public class EndGroupsException : Exception
    {
        public override string Message => "End of group iteration has reached";
    }
}
