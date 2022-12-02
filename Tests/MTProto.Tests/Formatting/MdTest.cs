using MTProto.Utils.Md;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MTProto.Tests.Formatting
{
    public class MdTest
    {
        [Fact]
        public void MdTest1()
        {
            var md = MdContainer.GetNormal(null, "Hello!");
            var entities = md.ToEntities();

            Assert.NotNull(entities);
        }
    }
}
