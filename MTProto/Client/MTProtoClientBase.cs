using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTProto.Client.Events;
using MTProto.Core.Database;
using MTProto.Utils.Md;

namespace MTProto.Client
{
    public abstract class MTProtoClientBase : IDisposable
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
        protected bool _isBot;

        #region events region
        public virtual EventManager<MTProtoClientBase, TL.UpdateShortSentMessage> EventShortSentMessage { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateAttachMenuBots> EventAttachMenuBots { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateBotCallbackQuery> EventBotCallbackQuery { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateBotChatInviteRequester> EventBotChatInviteRequester { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateBotCommands> EventBotCommands { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateBotInlineQuery> EventBotInlineQuery { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateBotInlineSend> EventBotInlineSend { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateBotMenuButton> EventBotMenuButton { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateBotPrecheckoutQuery> EventBotPrecheckoutQuery { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateBotShippingQuery> EventBotShippingQuery { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateBotStopped> EventBotStopped { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateBotWebhookJSON> EventBotWebhookJSON { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateBotWebhookJSONQuery> EventBotWebhookJSONQuery { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateChannelAvailableMessages> EventChannelAvailableMessages { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateChannelMessageForwards> EventChannelMessageForwards { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateChannelMessageViews> EventChannelMessageViews { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateNewChannelMessage> EventNewChannelMessage { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateNewEncryptedMessage> EventNewEncryptedMessage { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateNewMessage> EventNewMessage { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateNewScheduledMessage> EventNewScheduledMessage { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateNewStickerSet> EventNewStickerSet { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateEditChannelMessage> EventEditChannelMessage { get; set; }
        public virtual EventManager<MTProtoClientBase, TL.UpdateEditMessage> EventEditMessage { get; set; }
        
        #endregion

        public virtual IMTProtoDbProvider MTProtoDatabase { get; protected set; }

        public TL.User CachedMe { get; protected set; }
        internal WTelegram.Client wClient;

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

            switch (arg)
            {
                case TL.UpdateShortSentMessage update:
                    await (EventShortSentMessage?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateAttachMenuBots update:
                    await (EventAttachMenuBots?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateBotCallbackQuery update:
                    await (EventBotCallbackQuery?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateBotChatInviteRequester update:
                    await (EventBotChatInviteRequester?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateBotCommands update:
                    await (EventBotCommands?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateBotInlineQuery update:
                    await (EventBotInlineQuery?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateBotInlineSend update:
                    await (EventBotInlineSend?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateBotMenuButton update:
                    await (EventBotMenuButton?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateBotPrecheckoutQuery update:
                    await (EventBotPrecheckoutQuery?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateBotShippingQuery update:
                    await (EventBotShippingQuery?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateBotStopped update:
                    await (EventBotStopped?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateBotWebhookJSON update:
                    await (EventBotWebhookJSON?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateBotWebhookJSONQuery update:
                    await (EventBotWebhookJSONQuery?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateChannelAvailableMessages update:
                    await (EventChannelAvailableMessages?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateChannelMessageForwards update:
                    await (EventChannelMessageForwards?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateChannelMessageViews update:
                    await (EventChannelMessageViews?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateNewChannelMessage update:
                    await (EventNewChannelMessage?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateNewEncryptedMessage update:
                    await (EventNewEncryptedMessage?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateNewMessage update:
                    await (EventNewMessage?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateNewScheduledMessage update:
                    await (EventNewScheduledMessage?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateNewStickerSet update:
                    await (EventNewStickerSet?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateEditChannelMessage update:
                    await (EventEditChannelMessage?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
                case TL.UpdateEditMessage update:
                    await (EventEditMessage?.InvokeHandlers(this, update) ?? Task.CompletedTask);
                    return;
            }

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
                if (await MTProtoDatabase.GetPeerInfo(chat.Key) != null)
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
    }
}
