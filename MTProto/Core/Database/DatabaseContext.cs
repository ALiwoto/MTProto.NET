using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Reflection.Metadata;
using MTProto.Core.Database.Models;
using Microsoft.EntityFrameworkCore.Design;

namespace MTProto.Core.Database
{
    public sealed class DatabaseContext : DbContext, IMTProtoDbProvider
    {
        public string DbPath { get; set; }

        private bool isMigrated = false;


        public DbSet<PeerInfo> PeerInfos { get; set; }
        public DbSet<OwnerPeerInfo> OwnerInfos { get; set; }

        private readonly object _dbLock = new();
        public DatabaseContext()
        {

        }

        public DatabaseContext(string path)
        {
            DbPath = path ?? throw new ArgumentNullException(nameof(path));
        }

        public void SetPath(string path)
        {
            DbPath = path ?? throw new ArgumentNullException(nameof(path));
        }

        public void DoMigrate()
        {
            lock (_dbLock)
            {
                if (isMigrated)
                    return;
                Database.Migrate();
                isMigrated = true;
            }
        }

        public async Task DoMigrateAsync()
        {
            if (isMigrated)
                return;

            await Database.MigrateAsync();
            isMigrated = true;
        }

        public async Task<bool> VerifyOwner(OwnerPeerInfo peer, bool secondTime = false)
        {
            if (!isMigrated)
                return false;

            peer.OwnerId = peer.OwnerId.Replace("+", "");
            try
            {
                var theOwner = OwnerInfos.Find(peer.OwnerId);
                if  (theOwner != null && theOwner.OwnerId == peer.OwnerId)
                {
                    if (theOwner.AreSame(peer))
                    {
                        // are they exactly same?
                        return true;
                    }

                    // update the value inside of database
                    OwnerInfos.Update(peer);
                    await SaveChangesAsync();
                }
                else if (secondTime)
                {
                    throw new InvalidOperationException("Couldn't find owner after inserting.");
                }
            }
            catch (Exception ex)
            {
                if (secondTime || !ex.Message.Contains("no such column"))
                {
                    throw;
                }
            }

            lock (_dbLock)
            {
                OwnerInfos.Add(peer);
                SaveChanges();
            }

            return await VerifyOwner(peer, true);
        }

        public void SaveNewUser(long userId, long accessHash) =>
            SaveNewPeer(new PeerInfo()
            {
                PeerId = userId,
                AccessHash = accessHash,
                PeerType = PeerType.PeerTypeUser,
            });
        public void SaveNewChat(long chatId, long accessHash) =>
            SaveNewPeer(new PeerInfo()
            {
                PeerId = chatId,
                AccessHash = accessHash,
                PeerType = PeerType.PeerTypeChat,
            });
        public void SaveNewChannel(long channelId, long accessHash) =>
            SaveNewPeer(new PeerInfo()
            {
                PeerId = channelId,
                AccessHash = accessHash,
                PeerType = PeerType.PeerTypeChannel,
            });
        public void SaveNewPeer(PeerInfo info)
        {
            lock (_dbLock) 
            {
                PeerInfos.Add(info);
                SaveChanges();
            }

        }
        public async Task<PeerInfo> GetPeerInfo(long peerId) => 
            await PeerInfos.FindAsync(peerId);
        public int GetPeersCount(PeerType peerType) =>
            PeerInfos.Where(i => i.PeerType == peerType).Count();
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
        }



        public OwnerPeerInfo GetOwner(string ownerId)
        {
            if (!isMigrated)
            {
                return null;
            }

            try
            {
                var tmp = OwnerInfos.Find(ownerId.Replace("+", ""));
                return tmp;
            }
            catch { return null; }
        }
        public async Task<OwnerPeerInfo> GetOwnerAsync(string ownerId)
        {
            if (!isMigrated)
            {
                return null;
            }


            return await OwnerInfos.FindAsync(ownerId.Replace("+", ""));
        }

        public byte[] GetOwnerAuthData(string ownerId) => 
            GetOwner(ownerId)?.AuthKey;
        public async Task<byte[]> GetOwnerAuthDataAsync(string ownerId) =>
            (await GetOwnerAsync(ownerId))?.AuthKey;

        public void UpdateOwnerAuthKey(string ownerId, byte[] authKey)
        {
            if (!isMigrated)
            {
                return;
            }

            ownerId = ownerId.Replace("+", "");

            var theOwner = GetOwner(ownerId);
            if (theOwner == null)
            {
                lock (_dbLock)
                {
                    OwnerInfos.Add(new OwnerPeerInfo
                    {
                        AuthKey = authKey,
                        OwnerId = ownerId,
                    });
                    SaveChanges();
                }
                return;
            }

            lock (_dbLock)
            {
                theOwner.AuthKey = authKey;
                OwnerInfos.Update(theOwner);
                SaveChanges();
            }
        }
    }

}
