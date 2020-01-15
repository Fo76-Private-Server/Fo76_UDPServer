using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fo76_UDPServer
{
    public static class StreamExtensions
    {
        public static void WriteShort(this Stream output, ushort value)
        {
            byte[] buff = BitConverter.GetBytes(value);
            output.Write(buff, 0, buff.Length);
        }

        public static void WriteID(this Stream output, ulong value)
        {
            byte[] buff = BitConverter.GetBytes(value);
            output.Write(buff, 2, 6);
        }

        public static void Write3Bytes(this Stream output, int value)
        {
            byte[] buff = BitConverter.GetBytes(value);
            output.Write(buff, 1, 3);
        }

        public static ushort ReadShort(this Stream input)
        {
            byte[] shortBuff = new byte[2];
            input.Read(shortBuff, 0, shortBuff.Length);
            return BitConverter.ToUInt16(shortBuff, 0);
        }

        public static ulong ReadID(this Stream input)
        {
            byte[] IDBuff = new byte[8];
            input.Read(IDBuff, 2, 6);
            return BitConverter.ToUInt64(IDBuff, 0);
        }

        public static int Read3Bytes(this Stream input)
        {
            byte[] intBuff = new byte[4];
            input.Read(intBuff, 1, 3);
            return BitConverter.ToInt32(intBuff, 0);
        }
    }
}
