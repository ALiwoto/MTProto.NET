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
    public class DatabaseContext : DbContext
    {
        public string DbPath { get; set; }
        public long OwnerId { get; protected set; }

        public DbSet<PeerInfo> PeerInfos { get; set; }
        public DbSet<OwnerPeerInfo> OwnerInfos { get; set; }

        protected object _dbLock = new object();
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

        public virtual void SetOwnerId(long ownerId)
        {
            DbPath = $"mtproto_{ownerId}.db";
        }

        public virtual async Task DoMigrate()
        {
            await Database.MigrateAsync();
        }

        public virtual async Task<bool> VerifyOwner(bool isBot, bool secondTime = false)
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

        public virtual void SaveNewUser(long userId, long accessHash) =>
            SaveNewPeer(new PeerInfo()
            {
                PeerId = userId,
                AccessHash = accessHash,
            });
        public virtual void SaveNewPeer(PeerInfo info)
        {
            lock (_dbLock) 
            {
                PeerInfos.Add(info);
                SaveChanges();
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
        }


    }

}
