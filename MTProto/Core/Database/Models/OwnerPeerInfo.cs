﻿using System;
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
        public long OwnerId { get; set; }
        public byte[] AuthKey { get; set; }
        public bool IsBot { get; set; }
    }
}
