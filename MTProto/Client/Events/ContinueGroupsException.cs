using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTProto.Client.Events
{
    /// <summary>
    /// An exception type that gets thrown when and only when
    /// we want to continue the execution of handlers of next group.
    /// </summary>
    public class ContinueGroupsException : Exception
    {
        public override string Message => "Continue from the next groups handlers";
    }
}
