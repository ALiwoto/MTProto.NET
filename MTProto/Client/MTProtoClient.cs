using MTProto.Utils.Md;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using MTProto.Core.Database;
using MTProto.Client.Events;
using MTProto.Core.Database.Models;
using MTProto.Core.Errors;
using System.Text;

namespace MTProto.Client
{
    public class MTProtoClient : MTProtoClientBase
    {
        
        public MTProtoClient(
            string botToken = null, string apiId = null, 
            string apiHash = null, string phoneNumber = null,
            Func<string> verificationCodeProvider = null,
            Func<string> passphraseProvider = null,
            string firstName = null, string lastName = null,
            string password = null, string sessionName = null,
            string sessionKey = null, string serverAdsress = null,
            string deviceModel = null, string systemVersion = null,
            string systemLangCode = null,
            string appVersion = null,
            Stream sessionStore = null,
            IMTProtoDbProvider dbProvider = null,
            
            string sessionFileName = "mtproto_session.session")
        {
            if (!string.IsNullOrEmpty(sessionFileName) && 
                    !File.Exists(sessionFileName))
            {
                File.Create(sessionFileName)?.Close();
            }

            _botToken = botToken;
            if (!string.IsNullOrEmpty(botToken))
                ownerId = botToken.Split(":")[0];
            else
                ownerId = phoneNumber.Replace(" ", "");

            _apiId = apiId;
            _apiHash = apiHash;
            _phoneNumber = phoneNumber;
            _verificationCodeProvider = verificationCodeProvider;
            _passphraseProvider = passphraseProvider;
            _firstName = firstName;
            _lastName = lastName;
            _password = password;
            _sessionName = sessionName;
            _sessionKey = sessionKey;
            _serverAddress = serverAdsress;
            _deviceModel = deviceModel;
            _systemVersion = systemVersion;
            _appVersion = appVersion;
            _systemLangCode = systemLangCode;

            _sessionFileName = sessionFileName;
            MTProtoDatabase = dbProvider ?? 
                new DatabaseContext(sessionFileName);

            if (sessionStore != null)
                SessionStore = sessionStore;

            MTProtoDatabase.DoMigrate();
            _isBot = !string.IsNullOrEmpty(_botToken);
            wClient = new WTelegram.Client(
                configProvider: ConfigProvider,
                sessionStore: SessionStore);
        }

        public MTProtoClient(Func<string, string> configProvider,
            string sessionFileName = "mtproto_session.session")
        {
            _sessionFileName = sessionFileName;
            _isBot = !string.IsNullOrEmpty(_botToken);
            wClient = new WTelegram.Client(configProvider);
        }

        public virtual void SetDatabase<T>(T db) where T : IMTProtoDbProvider
        {
            MTProtoDatabase = db;
        }

        public virtual async Task<TL.User> Start(IMTProtoDbProvider db = null)
        {
            CachedMe = await LoginIfNeeded();

            if (db != null)
            {
                MTProtoDatabase = db;
            }
            else MTProtoDatabase ??= new DatabaseContext(_sessionFileName);
            
            
            var ownerInfo = new OwnerPeerInfo()
            {
                AuthKey = MTProtoDatabase.GetOwnerAuthData(ownerId),
                IsBot  = CachedMe.IsBot,
                OwnerId = ownerId,
            };
            if (!await MTProtoDatabase.VerifyOwner(ownerInfo))
            {
                if (MTProtoDatabase is not DatabaseContext)
                {
                    // is the database a custom type?
                    throw new InvalidOperationException("Failed to verify the owner account in database.");
                }

                // we have failed to verify the owner account, we will have to
                // recreate the db file.
                RecreateDatabase();
                if (!await MTProtoDatabase.VerifyOwner(ownerInfo))
                {
                    throw new InvalidOperationException("Failed to verify the owner account in database.");
                }
            }

            wClient.OnUpdate += RunUpdateHandlers;
            return CachedMe;
        }



        public virtual void RecreateDatabase()
        {
            // cache the path before disposing the db object.
            var path = MTProtoDatabase.DbPath;
            MTProtoDatabase.Dispose();
            File.Delete(path);
            MTProtoDatabase = new DatabaseContext(_sessionFileName);
        }

        public virtual async Task<TL.User> LoginIfNeeded() =>
            _isBot ? await wClient.LoginBotIfNeeded(_botToken) :
            await wClient.LoginUserIfNeeded();
        public virtual void Close() => Dispose();

        /// <summary>
        /// Sends a message to a certain chat, using its chatId.
        /// </summary>
        /// <param name="chatId">
        /// the chat id of the target chat. 
        /// Please do notice that if the chat is a supergroup/channel, the id
        /// should start with -100 (same as bot api and pyrogram).
        /// </param>
        /// <param name="text"></param>
        /// <param name="ReplyToMessageId"></param>
        /// <returns></returns>
        public override async Task SendMessage(long chatId, MdContainer text, int ReplyToMessageId = 0)
        {
            var txtEntities = text?.ToEntities();
            var theText = text?.Text;
            await wClient.SendMessageAsync(
                await GetInputPeer(chatId, false), 
                theText, 
                null, ReplyToMessageId, txtEntities, default, false);
        }
        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="text"></param>
        /// <param name="ReplyToMessageId"></param>
        /// <returns></returns>
        public override async Task SendMessage(TL.Peer chatId, MdContainer text, int ReplyToMessageId = 0)
        {
            var txtEntities = text?.ToEntities();
            var theText = text?.Text;
            await wClient.SendMessageAsync(
                await GetInputPeer(chatId),
                theText, null, ReplyToMessageId, txtEntities, default, false);
        }
        public override async Task SendMessage(string chatId, MdContainer text, int ReplyToMessageId = 0)
        {
            var txtEntities = text?.ToEntities();
            var theText = text?.Text;
            await wClient.SendMessageAsync(
                await GetInputPeer(chatId), 
                theText, null, ReplyToMessageId, txtEntities, default, false);
        }
        public override async Task<TL.InputPeer> GetInputPeer(TL.Peer chatId) =>
            chatId switch
            {
                TL.PeerUser => await GetInputPeer(chatId.ID, false),
                TL.PeerChannel => await GetInputPeer(chatId.ID, true),
                TL.PeerChat => await GetInputPeer(chatId.ID, false),
                _ => null,
            };
        public override async Task<TL.InputPeer> GetInputPeer(long chatId, bool needsFix)
        {
            var info = await MTProtoDatabase.GetPeerInfo(needsFix ? FixChatID(chatId) : chatId);

            return info == null
                ? throw new InvalidPeerIdException()
                : info.PeerType switch
            {
                PeerType.PeerTypeChannel => new TL.InputPeerChannel(chatId, info.AccessHash),
                PeerType.PeerTypeUser => new TL.InputPeerUser(chatId, info.AccessHash),
                PeerType.PeerTypeChat => new TL.InputPeerChat(chatId),
                PeerType.PeerTypeEmpty or
                PeerType.PeerTypeSelf or
                PeerType.PeerTypeUserFromMessage or
                PeerType.PeerTypeChannelFromMessage =>
                    throw new NotImplementedException($"{info.PeerType} not implemented"),
                _ => null,
            };
        }

        public async Task<TL.InputPeer> GetInputPeer(string chatId)
        {
            var results = await ResolveUsername(chatId);
            return results?.UserOrChat?.ToInputPeer();
        }


        public Task<TL.Contacts_ResolvedPeer> ResolveUsername(string username)
        {
            return wClient.Invoke(new TL.Methods.Contacts_ResolveUsername
            {
                username = username
            });
        }

        private string ConfigProvider(string key)
        {
            return key switch
            {
                "session_pathname" => _sessionFileName,
                "api_id" => _apiId,
                "api_hash" => _apiHash,
                "phone_number" => _phoneNumber,
                "verification_code" => _verificationCodeProvider?.Invoke(),
                "first_name" => _firstName,
                "last_name" => _lastName,
                "password" => _password ?? _passphraseProvider?.Invoke(),
                "session_key" => _sessionKey,
                "bot_token" => _botToken,
                "server_address" => _serverAddress,
                "device_model" => _deviceModel,
                "system_version" => _systemVersion,
                "app_version" => _appVersion,
                "system_lang_code" => _systemLangCode,
                "lang_pack" =>  _langPack,
                "lang_code" => _langCode,
                "" => null,
                _ => null,
            };
            ;
        }
        public override void Dispose()
        {
            wClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}