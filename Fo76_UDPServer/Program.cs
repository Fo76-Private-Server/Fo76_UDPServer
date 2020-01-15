using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using wolfSSL.CSharp;

namespace Fo76_UDPServer
{
    class Program
    {
        //ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ recieved over the websocket
        private static byte[] HashSecretServerPassword = new byte[] { 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A };

        public static uint my_psk_server_cb(IntPtr ssl, string identity, IntPtr key, uint max_key)
        {
            wolfssl.log(wolfssl.INFO_LOG, "PSK Client Identity = " + identity);

            Marshal.Copy(HashSecretServerPassword, 0, key, HashSecretServerPassword.Length);

            return (uint)HashSecretServerPassword.Length;
        }

        private static void clean(IntPtr ssl, IntPtr ctx)
        {
            wolfssl.free(ssl);
            wolfssl.CTX_free(ctx);
            wolfssl.Cleanup();
        }

        public static void Log(int lvl, StringBuilder msg)
        {
            Console.WriteLine("LOG " + msg.ToString());
        }

        public static void Main(string[] args)
        {
            IntPtr ctx;
            IntPtr ssl;

            wolfssl.loggingCb loggingCb = Log;
            wolfssl.SetLogging(loggingCb);

            wolfssl.psk_delegate psk_cb = new wolfssl.psk_delegate(my_psk_server_cb);

            byte[] buff = new byte[8192];
            byte[] stubReply = new byte[] { 0xe1, 0x6f, 0x80, 0xff, 0xbe, 0xdc, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02 };

            wolfssl.Init();

            Console.WriteLine("Calling ctx Init from wolfSSL");
            ctx = wolfssl.CTX_dtls_new(wolfssl.useDTLSv1_2_server());
            if (ctx == IntPtr.Zero)
            {
                Console.WriteLine("Error creating ctx structure");
                return;
            }

            Console.WriteLine("Finished init of ctx .... now load in cert and key");


            /* Test psk use with DHE */
            StringBuilder hint = new StringBuilder("PROJECT_76"); //Hardcoded hint in Fo76
            if (wolfssl.CTX_use_psk_identity_hint(ctx, hint) != wolfssl.SUCCESS)
            {
                Console.WriteLine("Error setting hint");
                wolfssl.CTX_free(ctx);
                return;
            }
            wolfssl.CTX_set_psk_server_callback(ctx, psk_cb);

            Console.Write("Setting cipher suite to ");
            StringBuilder set_cipher = new StringBuilder("TLS_PSK_WITH_AES_128_GCM_SHA256");
            Console.WriteLine(set_cipher);
            if (wolfssl.CTX_set_cipher_list(ctx, set_cipher) != wolfssl.SUCCESS)
            {
                Console.WriteLine("Failed to set cipher suite");
                wolfssl.CTX_free(ctx);
                return;
            }

            IPAddress ip = IPAddress.Parse("127.0.0.1");
            UdpClient udp = new UdpClient(3001);
            IPEndPoint ep = new IPEndPoint(ip, 3001);
            Console.WriteLine("Started UDP and waiting for a connection");

            ssl = wolfssl.new_ssl(ctx);
            if (ssl == IntPtr.Zero)
            {
                Console.WriteLine("Error creating ssl object");
                udp.Close();
                wolfssl.CTX_free(ctx);
                return;
            }

            if (wolfssl.set_dtls_fd(ssl, udp, ep) != wolfssl.SUCCESS)
            {
                Console.WriteLine(wolfssl.get_error(ssl));
                udp.Close();
                clean(ssl, ctx);
                return;
            }

            if (wolfssl.accept(ssl) != wolfssl.SUCCESS)
            {
                Console.WriteLine(wolfssl.get_error(ssl));
                udp.Close();
                clean(ssl, ctx);
                return;
            }

            /* print out results of TLS/SSL accept */
            Console.WriteLine("SSL version is " + wolfssl.get_version(ssl));
            Console.WriteLine("SSL cipher suite is " + wolfssl.get_current_cipher(ssl));

            /* get connection information and print ip - port */
            wolfssl.DTLS_con con = wolfssl.get_dtls_fd(ssl);
            Console.Write("Connected to ip ");
            Console.Write(con.ep.Address.ToString());
            Console.Write(" on port ");
            Console.WriteLine(con.ep.Port.ToString());

            /* read information sent and send a reply */

            while(true) {
                int readSize = wolfssl.read(ssl, buff, buff.Length);
                if (readSize <= 0) {
                    continue;
                }

                Console.WriteLine("Received Msg");
                Console.WriteLine(ByteArrayToString(buff));
                MessageHandler.OnMessage(ssl, buff, readSize);

                Thread.Sleep(50);
            }

            /*if (wolfssl.read(ssl, buff, 1023) < 0)
            {
                Console.WriteLine("Error reading message");
                Console.WriteLine(wolfssl.get_error(ssl));
                udp.Close();
                clean(ssl, ctx);
                return;
            }
            Console.WriteLine(ByteArrayToString(buff));
            stubReply[0] = buff[0];
            stubReply[1] = buff[1];
            if (wolfssl.write(ssl, stubReply, stubReply.Length) != stubReply.Length)
            {
                Console.WriteLine("Error writing message");
                Console.WriteLine(wolfssl.get_error(ssl));
                udp.Close();
                clean(ssl, ctx);
                return;
            }*/

            Console.WriteLine("At the end freeing stuff");
            wolfssl.shutdown(ssl);
            udp.Close();
            clean(ssl, ctx);
        }

        public static string ByteArrayToString(byte[] data)
        {
            string retVar = "";
            foreach (var i in data)
            {
                if (i.ToString("X").Length < 2)
                    retVar = retVar + "0" + i.ToString("X") + " ";
                else
                    retVar = retVar + i.ToString("X") + " ";
            }
            return retVar;
        }
    }
}
