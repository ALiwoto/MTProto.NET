using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MTProto.Core.Database;
using MTProto.Utils.Md;

namespace MTProto.Client
{
    public abstract class MTProtoClientBase : MTProtoEventsClient, IDisposable
    {
        protected string _sessionName;
        protected string _sessionFileName;
        protected string _botToken;
        protected string _apiId;
        protected string _apiHash;
        protected string _phoneNumber;
        protected Func<string> _verificationCodeProvider;
        protected Func<string> _passphraseProvider;
        protected string _firstName;
        protected string _lastName;
        protected string _password;
        protected string _sessionKey;
        protected string _serverAddress;
        protected string _deviceModel;
        protected string _systemVersion;
        protected string _appVersion;
        protected string _systemLangCode;
        protected string _langPack;
        protected string _langCode;
        protected bool _isBot;

        

        public virtual IMTProtoDbProvider MTProtoDatabase { get; protected set; }

        public TL.User CachedMe { get; protected set; }

        private Stream _sessionStore;

        /// <summary>
        /// This is phone number in case of users and bot id
        /// in case of bots.
        /// </summary>
        protected string ownerId;
        
        internal WTelegram.Client wClient;

        protected internal virtual Stream SessionStore
        {
            get
            {
                _sessionStore ??= 
                    new CustomSessionStore(MTProtoDatabase, ownerId);
                return _sessionStore;
            }
            set { _sessionStore = value; }
        }


        public abstract Task SendMessage(long chatId, MdContainer text, int ReplyToMessageId = 0);
        public abstract Task SendMessage(string chatId, MdContainer text, int ReplyToMessageId = 0);
        public abstract Task SendMessage(TL.Peer chatId, MdContainer text, int ReplyToMessageId = 0);
        public abstract Task<TL.InputPeer> GetInputPeer(long chatId, bool isUser);
        public abstract Task<TL.InputPeer> GetInputPeer(TL.Peer chatId);

        public abstract void Dispose();

        #region event-related methods
        protected internal virtual async Task RunUpdateHandlers(TL.IObject arg)
        {
            if (arg is TL.Updates updates)
            {
                foreach (var current in updates.updates)
                {
                    await StoreAccessHashes(updates.Users);
                    await StoreAccessHashes(updates.Chats);
                    await RunUpdateHandlers(current);
                }

                return;
            }

            await HandleValidEvent(this, arg);
            return;
        }
        protected internal virtual async Task StoreAccessHashes(Dictionary<long, TL.User> users)
        {
            foreach (var user in users)
            {
                if (await MTProtoDatabase.GetPeerInfo(user.Key) != null)
                {
                    continue;
                }

                MTProtoDatabase.SaveNewUser(user.Key, user.Value.access_hash);
            }
        }
        protected internal virtual async Task StoreAccessHashes(Dictionary<long, TL.ChatBase> chats)
        {
            foreach (var chat in chats)
            {
                var fixedId = FixChatID(chat.Key);
                if (await MTProtoDatabase.GetPeerInfo(fixedId) != null)
                {
                    continue;
                }

                // Derived classes: TL.ChatEmpty, TL.Chat, TL.ChatForbidden, TL.Channel, TL.ChannelForbidden
                switch (chat.Value)
                {
                    case TL.ChannelForbidden:
                    case TL.ChatForbidden:
                    case TL.ChatEmpty:
                    case TL.Chat:
                        return;
                    case TL.Channel channel:
                        MTProtoDatabase.SaveNewChannel(FixChatID(channel.ID), channel.access_hash);
                        return;
                }

                //MTProtoDatabase.SaveNewUser(chat.Key, chat.Value.);
            }

        }
        #endregion
        public virtual long FixChatID(long chatId) =>
            Convert.ToInt64("-100" + Math.Abs(chatId));


        #region store class
        private class CustomSessionStore: Stream
        {
            internal IMTProtoDbProvider dbProvider;
            public string OwnerId { get; set; }
            public CustomSessionStore(IMTProtoDbProvider provider, string ownerId) 
            {
                dbProvider = provider ?? throw new ArgumentNullException(nameof(provider));
                OwnerId = ownerId ?? throw new ArgumentNullException(nameof(ownerId));
            }

            public override int Read(byte[] buffer, int offset, int count)
            {

                var authKey = dbProvider.GetOwnerAuthData(OwnerId);
                if (authKey == null || authKey.Length == 0)
                    return 0;

                Array.Copy(authKey, 0, buffer, offset, count);
                return count;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                var authData = count == buffer.Length ? 
                    buffer : buffer[offset..(offset + count)];
                if (authData.Length > 0)
                {
                    dbProvider.UpdateOwnerAuthKey(OwnerId, authData);
                }
            }

            public override long Length => 
                dbProvider.GetOwnerAuthData(OwnerId)?.Length ?? 0;
            public override long Position { get => 0; set { } }
            public override bool CanSeek => false;
            public override bool CanRead => true;
            public override bool CanWrite => true;
            public override long Seek(long offset, SeekOrigin origin) => 0;
            public override void SetLength(long value) { }
            public override void Flush()
            {
                Console.WriteLine("Flushed!");
            }
        }
        #endregion
    }
}
