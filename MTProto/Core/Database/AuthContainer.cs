using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace MTProto.Core.Database
{
    public class AuthContainer
    {
        public virtual byte[] Container { get; set; }
        public virtual IMTProtoDbProvider DbProvider { get; set; }
        public virtual long OwnerId { get; set; }

        public virtual long Length => Container.Length;
        public virtual Action<long, byte[]> UpdateAction { get; set; }
        

        public AuthContainer(byte[] container = null)
        {
            Container = container ?? Array.Empty<byte>();
        }

        public virtual byte[]ToBytes() => Container;

        public virtual void SetOwnerAuthData(byte[] buffer, int offset, int count)
        {
            Container = count == buffer.Length ? buffer : buffer[offset..(offset + count)];

            UpdateAction?.Invoke(OwnerId, Container);
        }
        public virtual void Flush()
        {
            
        }

        public virtual bool HasUnflushedAuthData() => Container.Length != 0;
    }
}
