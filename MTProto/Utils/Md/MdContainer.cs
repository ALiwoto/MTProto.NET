using MTProto.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTProto.Utils.Md
{
    public class MdContainer
    {
        public string Text { get; set; }
        private readonly MTProtoClientBase _hostClient;

        public MdContainer(MTProtoClientBase client)
        {
            _hostClient = client;
        }
        public MdContainer Bold(string text) =>
            AddWithFormatting(text, (theValue) => "*" + theValue + "*");
        public MdContainer Italic(string text) =>
            AddWithFormatting(text, (theValue) => "__" + theValue + "__");
        public MdContainer Normal(string text) => 
            AddWithFormatting(text, null);
        public MdContainer AddWithFormatting(string text, Func<string, string> formatter)
        {
            var result = TL.Markdown.Escape(text);
            if (formatter != null)
            {
                result = formatter.Invoke(TL.Markdown.Escape(text));
            }

            Text += result;
            return this;
        }



        public static MdContainer GetNormal(MTProtoClientBase client, string text) =>
            new MdContainer(client).Normal(text);

        public TL.MessageEntity[] ToEntities(bool premium = false) 
        {
            var tmp = Text;
            var entities = TL.Markdown.MarkdownToEntities(_hostClient?.wClient, ref tmp, premium);
            Text = tmp;
            return entities;
        }
    }
}
