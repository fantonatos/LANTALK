using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Threading;

using SharedObjects;

namespace MessageClient
{
    class ClientProgram
    {
        private static TcpClient Client = new TcpClient();

        static void Main(string[] args)
        {
            Console.Write("Enter the Server IP address: ");
            string serverHostName = Console.ReadLine();


            Client.Connect(NetUtility.GetHostAddressIPv4(serverHostName), 1234);
            NetworkStream stream = Client.GetStream();

            Console.Write("Pick a Nickname: ");
            byte[] nickname = Encoding.UTF8.GetBytes(Console.ReadLine());
            byte[] composedMessage = ComposeMessage(TransmissionType.Nickname, nickname);
            stream.Write(composedMessage, 0, composedMessage.Length);
            
            Thread receiverThread = new Thread(Receive);
            receiverThread.Start();
            
            while (true)
            {
                Console.Write("Message: ");
                ChatMessage msg = new ChatMessage(Console.ReadLine());
                Console.Write("To: ");
                msg.Destination = Console.ReadLine();

                // serialize the ChatMessage object and compose the server request
                byte[] serialized = msg.ToByteArray();
                byte[] concatenated = ComposeMessage(TransmissionType.Message, serialized);

                stream.Write(concatenated, 0, concatenated.Length);
                stream.Flush();
            }

            stream.Close();
            Client.Close();
            Console.WriteLine("Ping message sent. Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Compose the byte buffer that will be transmitted.
        /// </summary>
        private static byte[] ComposeMessage(TransmissionType Code, byte[] buffer) 
            => new byte[] { (byte)Code }.Concat(new byte[] { (byte)buffer.Length }).Concat(buffer).ToArray(); // [servercode][nickname length][nickname bytes]

        public static void Receive()
        {
            NetworkStream stream = Client.GetStream();

            while (true)
            {
                // the first byte we recieve tells us how long the message is.
                byte[] readAmt = new byte[1];
                stream.Read(readAmt, 0, 1);

                if (readAmt[0] == 0)
                    continue;

                // read that many bytes from the stream
                byte[] buffer = new byte[readAmt[0]];
                stream.Read(buffer, 0, buffer.Length);

                // deserialize the message object
                ChatMessage msg = ChatMessage.FromByteArray(buffer);

                Console.WriteLine($"\n{msg.Source} ~> {msg}");
            }
        }

        
    }
}
