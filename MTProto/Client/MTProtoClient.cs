using MTProto.Utils.Md;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using TL;

namespace MTProto.Client
{
    public class MTProtoClient : IDisposable
    {
        protected string _sessionName;
        protected string _sessionFileName;
        protected string _botToken;
        protected string _apiId;
        protected string _apiHash;
        protected string _phoneNumber;
        protected Func<string> _verificationCodeProvider;
        protected string _firstName;
        protected string _lastName;
        protected string _password;
        protected string _sessionKey;
        protected string _serverAddress;
        protected string _deviceModel;


        internal WTelegram.Client wClient;

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
            wClient = new WTelegram.Client(configProvider: ConfigProvider);
        }

        public MTProtoClient(Func<string, string> configProvider,
            string sessionFileName = "mtproto_session.session")
        {
            _sessionFileName = sessionFileName;
            wClient = new WTelegram.Client(configProvider);
        }

        public virtual async Task<User> Start()
        {
            if (!string.IsNullOrEmpty(_botToken))
            {
                return await wClient.LoginBotIfNeeded(_botToken);
            }

            return await wClient.LoginUserIfNeeded();
        }
        public virtual void Close() => Dispose();

        public async void SendMessage(long chatId, MdContainer text)
        {
            await wClient.SendMessageAsync(await GetInputPeer(chatId), text?.Text, null, 0, text?.ToEntities(), default, false);
        }

        public async Task SendMessage(string chatId, MdContainer text)
        {
            var txtEntities = text?.ToEntities();
            var theText = text?.Text;
            await wClient.SendMessageAsync(await GetInputPeer(chatId), theText, null, 0, txtEntities, default, false);
        }

        public async Task<InputPeer> GetInputPeer(long chatId)
        {
            return null;
        }

        public async Task<InputPeer> GetInputPeer(string chatId)
        {
            var results = await wClient?.Contacts_ResolveUsername(chatId);
            return results?.UserOrChat?.ToInputPeer();
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
        public void Dispose()
        {
            wClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}