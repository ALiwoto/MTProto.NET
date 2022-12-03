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
    }
}
