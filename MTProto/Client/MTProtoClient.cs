using MTProto.Utils.Md;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using MTProto.Core.Database;
using MTProto.Client.Events;
using MTProto.Core.Database.Models;

namespace MTProto.Client
{
    public class MTProtoClient : MTProtoClientBase
    {
        
        public MTProtoClient(
            string botToken = null, string apiId = null, 
            string apiHash = null, string phoneNumber = null,
            Func<string> verificationCodeProvider = null,
            string firstName = null, string lastName = null,
            string password = null, string sessionName = null,
            string sessionKey = null, string serverAdsress = null,
            string deviceModel = null,
            
            
            string sessionFileName = "mtproto_session.session")
        {
            if (!Directory.Exists(sessionFileName))
            {
                File.Create(sessionFileName)?.Close();
            }
            _botToken = botToken;
            _apiId = apiId;
            _apiHash = apiHash;
            _phoneNumber = phoneNumber;
            _verificationCodeProvider = verificationCodeProvider;
            _firstName = firstName;
            _lastName = lastName;
            _password = password;
            _sessionName = sessionName;
            _sessionKey = sessionKey;
            _serverAddress = serverAdsress;
            _deviceModel = deviceModel;


            _sessionFileName = sessionFileName;
            _isBot = !string.IsNullOrEmpty(_botToken);
            wClient = new WTelegram.Client(configProvider: ConfigProvider);
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

            MTProtoDatabase = db ?? new DatabaseContext(CachedMe.ID);

            await MTProtoDatabase.DoMigrate();
            if (!await MTProtoDatabase.VerifyOwner(_isBot))
            {
                // we have failed to verify the owner account, we will have to
                // recreate the db file.
                RecreateDatabase();
                if (!await MTProtoDatabase.VerifyOwner(_isBot))
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
            MTProtoDatabase = new DatabaseContext(CachedMe.ID);
        }

        public virtual async Task<TL.User> LoginIfNeeded() =>
            _isBot ? await wClient.LoginBotIfNeeded(_botToken) :
            await wClient.LoginUserIfNeeded();
        public virtual void Close() => Dispose();

        public async void SendMessage(long chatId, MdContainer text, int ReplyToMessageId = 0)
        {
            var txtEntities = text?.ToEntities();
            var theText = text?.Text;
            await wClient.SendMessageAsync(
                await GetInputPeer(chatId), 
                theText, 
                null, ReplyToMessageId, txtEntities, default, false);
        }

        public async Task SendMessage(string chatId, MdContainer text, int ReplyToMessageId = 0)
        {
            var txtEntities = text?.ToEntities();
            var theText = text?.Text;
            await wClient.SendMessageAsync(
                await GetInputPeer(chatId), 
                theText, null, ReplyToMessageId, txtEntities, default, false);
        }

        public async Task<TL.InputPeer> GetInputPeer(long chatId)
        {
            var info = await MTProtoDatabase.GetPeerInfo(FixChatID(chatId));
            return info.PeerType switch
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
            switch (key)
            {
                case "session_pathname": return _sessionFileName;
                case "api_id": return _apiId;
                case "api_hash": return _apiHash;
                case "phone_number": return _phoneNumber;
                case "verification_code": return _verificationCodeProvider?.Invoke();
                case "first_name": return _firstName;
                case "last_name": return _lastName;
                case "password": return _password;
                case "session_key": return _sessionKey;
                case "bot_token": return _botToken;
                case "server_address": return _serverAddress;
                case "device_model": return _deviceModel;
                case "": return null;
                default: return null;
            };
        }
        public override void Dispose()
        {
            wClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}