using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MTProto.Core.Database.Models
{
    public class OwnerPeerInfo
    {
        [Key]
        public string OwnerId { get; set; }
        public byte[] AuthKey { get; set; }
        public bool IsBot { get; set; }


        public bool AreSame(OwnerPeerInfo other)
        {
            if (this == other) return true;

            if (this == null) return false;

            if (OwnerId != other.OwnerId) return false;

            if (AuthKey == null || AuthKey.Length != other.AuthKey.Length) 
                return false;

            return AuthKey.SequenceEqual(other.AuthKey);
        }
    }
}
