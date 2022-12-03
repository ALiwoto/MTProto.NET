using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTProto.Core.Database.Models;

namespace MTProto.Core.Database
{
    public interface IMTProtoDbProvider: IDisposable
    {
        string DbPath { get; set; }
        long OwnerId { get; protected set; }
        DbSet<PeerInfo> PeerInfos { get; set; }
        DbSet<OwnerPeerInfo> OwnerInfos { get; set; }

        Task DoMigrate();
        Task<bool> VerifyOwner(bool isBot, bool secondTime = false);
        void SaveNewUser(long userId, long accessHash);
        void SaveNewPeer(PeerInfo info);
    }
}
