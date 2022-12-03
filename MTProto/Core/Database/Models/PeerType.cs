using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTProto.Core.Database.Models
{
    public enum PeerType
    {
        /// <summary>
        /// 
        /// </summary>
        PeerTypeEmpty,

        /// <summary>
        /// 
        /// </summary>
        PeerTypeSelf,

        /// <summary>
        /// 
        /// </summary>
        PeerTypeChat,

        /// <summary>
        /// 
        /// </summary>
        PeerTypeUser,

        /// <summary>
        /// 
        /// </summary>
        PeerTypeChannel,

        /// <summary>
        /// 
        /// </summary>
        PeerTypeUserFromMessage,

        /// <summary>
        /// 
        /// </summary>
        PeerTypeChannelFromMessage,
    }
}
