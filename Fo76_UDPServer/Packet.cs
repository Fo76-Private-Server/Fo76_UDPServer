using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fo76_UDPServer
{
    public enum MessageType : byte {
        Token = 0,
        PingSend = 0x80,
        PingReply = 0x81,
        Message = 0x82
    }

    class Packet
    {
        public uint SessionId;
        public MessageType Type;

        private byte[] OriginalData;

        public Packet(byte[] data, int length) {
            this.OriginalData = new byte[length];
            Array.Copy(data, 0, this.OriginalData, 0, length);

            MemoryStream packetStream = new MemoryStream(data);
            this.SessionId = packetStream.ReadShort();
            this.Type = (MessageType)(packetStream.ReadByte());
        }
    }
}
