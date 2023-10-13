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
        DbSet<PeerInfo> PeerInfos { get; set; }
        DbSet<OwnerPeerInfo> OwnerInfos { get; set; }

        void DoMigrate();
        Task DoMigrateAsync();
        Task<bool> VerifyOwner(OwnerPeerInfo peer, bool secondTime = false);
        void SaveNewUser(long userId, long accessHash);
        void SaveNewChat(long chatId, long accessHash);
        void SaveNewChannel(long channelId, long accessHash);
        void SaveNewPeer(PeerInfo info);
        void UpdateOwnerAuthKey(string ownerId, byte[] authKey);

        byte[] GetOwnerAuthData(string ownerId);
        public Task<byte[]> GetOwnerAuthDataAsync(string ownerId);
        Task<PeerInfo> GetPeerInfo(long peerId);
    }
}
