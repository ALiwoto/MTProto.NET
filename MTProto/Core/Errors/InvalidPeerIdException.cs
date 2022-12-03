using System;

namespace MTProto.Core.Errors
{
    public class InvalidPeerIdException : Exception
    {
        public override string Message => 
            $"[{Code} PEER_ID_INVALID] - The peer id being used is invalid " +
            "or not known yet. Make sure you meet the peer before interacting with it";
        public int Code { get; set; }
        public InvalidPeerIdException(int code = 400)
        {
            Code = code;
        }

    }
}
