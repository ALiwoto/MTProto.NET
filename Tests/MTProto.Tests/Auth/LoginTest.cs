using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Buffers.Text;
using System.Text;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MTProto.Client;
using MTProto.Client.Events;
using MTProto.Utils.Md;
using Xunit;
using System.Xml.Serialization;

namespace MTProto.Tests.Auth
{
    public class LoginTest
    {
        private static HostApplicationBuilder? _appBuilder;
        private static IConfiguration? _config;
        public static IConfiguration Config
        {
            get
            {
                if (_appBuilder == null)
                {
                    _appBuilder = Host.CreateApplicationBuilder();
                    _config = _appBuilder.Configuration
                        .AddIniFile("appsettings.ini", true, true)
                        .AddIniFile("Tests/MTProto.Tests/appsettings.ini", true, true)
                        .Build();
                }

                return _config!;
            }
        }
        [Fact]
        public async void Test1()
        {
            //TODO: load these variables from env or from config file.
            var client = new MTProtoClient(
                deviceModel: "windows 11",
                botToken: Config.GetSection("main").GetValue<string>("bot_token"),
                phoneNumber: Config.GetSection("main").GetValue<string>("phone_number"),
                verificationCodeProvider: () =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var code = Config.GetSection("main").GetValue<string>("verification_code");
                        if (string.IsNullOrEmpty(code))
                            Thread.Sleep(3000);
                        else
                            return code;
                    }

                    return "";
                },
                passphraseProvider: () => Config.GetSection("main").GetValue<string>("passphrase"),
                apiId: Config.GetSection("main").GetValue<string>("api_id"),
                apiHash: Config.GetSection("main").GetValue<string>("api_hash")
            );
            Assert.NotNull(await client.Start());
            var md = MdContainer.GetNormal(client, "hello ! ,,").Bold("\nHow are you doing??");
            await client.SendMessage("Falling_inside_the_black", md);

            client.EventNewMessage += NewMessageHandler;
            
            client.EventNewChannelMessage += NewChannelMessageHandler;
            client.EventShortMessage += NewShortMessageHandler;



            Thread.Sleep(1000000);

            client.Close();
        }

        [Fact]
        public async void TestGetAllPeerInfos()
        {
            //TODO: load these variables from env or from config file.
            var client = new MTProtoClient(
                botToken: Config.GetSection("main").GetValue<string>("bot_token"),
                apiId: Config.GetSection("main").GetValue<string>("api_id"),
                apiHash: Config.GetSection("main").GetValue<string>("api_hash")
            );

            Assert.NotNull(await client.Start());

            var infos = client.MTProtoDatabase.PeerInfos.FromSql($"SELECT * FROM PeerInfos")
                .ToList();

            Assert.NotNull(infos);

            infos = client.MTProtoDatabase.PeerInfos.FromSql(
                $"SELECT * FROM PeerInfos WHERE PeerId = 1341091260"
            ).ToList();


            Assert.NotNull(infos);

            Console.WriteLine(infos);
        }

        private static async Task NewMessageHandler(MTProtoClientBase c, TL.UpdateNewMessage update)
        {
            if (update.message is TL.Message msg)
            {
                if (update.message.From?.ID == c.CachedMe?.ID)
                    return;
                //await c.SendMessage(update.message.Peer, MdContainer.GetNormal(c, "got your message!"), msg.ID);
                Console.WriteLine(msg.message);
            }
        }

        [AttributeUsage(AttributeTargets.All)]
        public class WotoAttribute: Attribute
        {
            public string? Name { get; set; }
        }


        [WotoAttribute(Name = "hello")]
        private static async Task NewChannelMessageHandler(MTProtoClientBase c, TL.UpdateNewChannelMessage update)
        {
            if (update.message is TL.Message msg)
            {
                if (update.message.From?.ID == c.CachedMe?.ID)
                    return;

                if (msg.From != null && msg.From.ID == 1341091260)
                    await c.SendMessage(update.message.Peer, MdContainer.GetNormal(c, "got your message!"), msg.ID);

                Console.WriteLine(msg.message);
            }
        }

        [WotoAttribute(Name = "hello")]
        private static async Task NewShortMessageHandler(MTProtoClientBase c, TL.UpdateShortMessage update)
        {
            Console.WriteLine(update.message);
        }
    }
}
