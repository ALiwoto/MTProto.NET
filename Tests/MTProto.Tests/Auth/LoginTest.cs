
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
            var client = new MTProtoClient(
                botToken: "",
                apiId: "",
                apiHash: ""
                );
            Assert.NotNull(await client.Start());
            var md = MdContainer.GetNormal(client, "hello ! ,,").Bold("\nHow are you doing??");
            await client.SendMessage("Falling_inside_the_black", md);

            client.EventNewMessage += NewMessageHandler;

            Thread.Sleep(1000000);

            client.Close();
        }

        private static async Task NewMessageHandler(MTProtoClientBase c, TL.UpdateNewMessage update)
        {
            if (update.message is TL.Message msg)
            {
                await c.SendMessage(msg.From.ID, MdContainer.GetNormal(c, "got your message!"), msg.ID);
                Console.WriteLine(msg.message);
            }
            return;
        }
}
}
