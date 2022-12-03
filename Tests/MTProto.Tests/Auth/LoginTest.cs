
using Microsoft.EntityFrameworkCore;
using MTProto.Client;
using MTProto.Client.Events;
using MTProto.Utils.Md;

namespace MTProto.Tests.Auth
{
    public class LoginTest
    {
        [Fact]
        public async void Test1()
        {
            //TODO: load these variables from env or from config file.
            var client = new MTProtoClient(
                botToken: "",
                apiId: "",
                apiHash: ""
            );
            Assert.NotNull(await client.Start());
            var md = MdContainer.GetNormal(client, "hello ! ,,").Bold("\nHow are you doing??");
            await client.SendMessage("Falling_inside_the_black", md);

            client.EventNewMessage += NewMessageHandler;
            client.EventNewChannelMessage += NewChannelMessageHandler;

            Thread.Sleep(1000000);

            client.Close();
        }

        [Fact]
        public async void TestGetAllPeerInfos()
        {
            //TODO: load these variables from env or from config file.
            var client = new MTProtoClient(
                botToken: "",
                apiId: "",
                apiHash: ""
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
                await c.SendMessage(update.message.Peer, MdContainer.GetNormal(c, "got your message!"), msg.ID);
                Console.WriteLine(msg.message);
            }
        }

        private static async Task NewChannelMessageHandler(MTProtoClientBase c, TL.UpdateNewChannelMessage update)
        {
            if (update.message is TL.Message msg)
            {
                if (update.message.From?.ID == c.CachedMe?.ID)
                    return;
                await c.SendMessage(update.message.Peer, MdContainer.GetNormal(c, "got your message!"), msg.ID);
                Console.WriteLine(msg.message);
            }
        }
    }
}
