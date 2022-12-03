using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTProto.Core.Database.Models
{
    public class PeerInfo
    {
        [Key]
        public long PeerId { get; set; }
        public long AccessHash { get; set; }
        public PeerType PeerType { get; set; }

        public long GetRealID()
        {
            if (PeerType == PeerType.PeerTypeChannel)
            {
                // "-100" == 4
                var idStr = PeerId.ToString();
                return idStr.StartsWith("-100") ? Convert.ToInt64(PeerId.ToString()[..4]) : PeerId;
            }

            return PeerId;
        }
    }
}
