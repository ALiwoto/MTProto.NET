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
        public long OwnerId { get; set; }

        public DbSet<PeerInfo> PeerInfos { get; set; }
        public DbSet<OwnerPeerInfo> OwnerInfos { get; set; }

        private readonly object _dbLock = new();
        public DatabaseContext(long ownerId = 0)
        {
            OwnerId = ownerId;
            SetOwnerId(ownerId);
        }

        public DatabaseContext(string path, long ownerId)
        {
            OwnerId = ownerId;
            DbPath = path;
        }

        public void SetOwnerId(long ownerId)
        {
            DbPath = $"mtproto_{ownerId}.db";
        }

        public async Task DoMigrate()
        {
            await Database.MigrateAsync();
        }

        public async Task<bool> VerifyOwner(bool isBot, bool secondTime = false)
        {
            try
            {
                var theOwner = OwnerInfos.Find(OwnerId);
                return theOwner.IsBot == isBot;
            }
            catch (Exception ex)
            {
                if (secondTime || !ex.Message.Contains("no such column"))
                {
                    throw;
                }

                lock (_dbLock)
                {
                    OwnerInfos.Add(new OwnerPeerInfo()
                    {
                        OwnerId = OwnerId,
                        IsBot = isBot,
                    });
                    SaveChanges();
                }
                
                return await VerifyOwner(isBot, true);
            }
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
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
        }


    }

}
