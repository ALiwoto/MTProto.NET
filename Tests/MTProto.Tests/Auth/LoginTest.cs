
using MTProto.Client;
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

            client.Close();
        }
    }
}
