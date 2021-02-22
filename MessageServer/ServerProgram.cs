using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using SharedObjects;

namespace MessageServer
{
    class ServerProgram
    {
        private static List<ClientHandler> Clients;

        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(NetUtility.GetLocalIPAddress(), 1234);
            server.Start();
            Clients = new List<ClientHandler>();
            
            Console.WriteLine($"Waiting for clients on {server.Server.LocalEndPoint}");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                // Assign the client to a handler
                Clients.Add(new ClientHandler(client));
                Console.WriteLine($"Accepted and assigned handler to {client.Client.RemoteEndPoint}.");
            }


            Console.WriteLine("Sever Execution Ended. Press any key to continue...");
            Console.ReadKey();
        }

        private class ClientHandler
        {

            private string Nickname;

            private TcpClient Client;

            public ClientHandler(TcpClient client)
            {
                Client = client;

                Thread workerThread = new Thread(Worker);
                workerThread.Start();
            }

            public void Send(ChatMessage message)
            {
                NetworkStream stream = Client.GetStream();

                // serialize msg
                byte[] serialized = message.ToByteArray();
                byte[] length = new byte[] { (byte)serialized.Length };
                byte[] concatenated = new byte[serialized.Length + length.Length];

                Array.Copy(length, concatenated, 1);
                Array.Copy(serialized, 0, concatenated, 1, serialized.Length);

                stream.Write(concatenated, 0, concatenated.Length);
                stream.Flush();
            }

            public void Worker()
            {
                NetworkStream stream = Client.GetStream();

                while (true)
                {
                    // the first byte is a command telling the server what to expect
                    byte[] commands = new byte[1];
                    try
                    {
                        stream.Read(commands, 0, 1);
                    }
                    catch (System.IO.IOException except)
                    {
                        Console.WriteLine($"{Client.Client.RemoteEndPoint} ({Nickname}) has disconnected.");
                        Clients.Remove(this);
                        stream.Dispose();
                        Client.Close();
                        Client.Dispose();
                        break;
                    }
                    

                    if (commands[0] == 0)   
                        continue; // no command recieved, do not proceed

                    TransmissionType code = (TransmissionType)commands[0];

                    switch (code)
                    {
                        // The client wants to send a message
                        case TransmissionType.Message:

                            // the second byte we recieve tells us how long the message is
                            byte[] readAmt = new byte[1];
                            stream.Read(readAmt, 0, 1);

                            // read that many bytes from the stream
                            byte[] buffer = new byte[readAmt[0]];
                            stream.Read(buffer, 0, buffer.Length);

                            // deserialize the message object
                            ChatMessage msg = ChatMessage.FromByteArray(buffer);

                            // find the destination client
                            bool resolved = false;
                            foreach (ClientHandler remoteClient in Clients)
                            {
                                if (remoteClient.Nickname == msg.Destination)
                                {
                                    // we found our destination, send him the message
                                    msg.Source = Nickname;
                                    remoteClient.Send(msg);
                                    Console.WriteLine($"Routed {msg.Source} ~> {msg.Destination}");
                                    resolved = true;
                                    break;
                                }
                            }

                            if (!resolved)
                                Console.WriteLine("Error: Message routed to a non-existing client.");
                            break;

                        // The client want to set his nickname
                        case TransmissionType.Nickname:

                            string oldNickname = Nickname;

                            // the client wants to set his nickname
                            // this byte tells us how long the message is.
                            byte[] amount = new byte[1];
                            stream.Read(amount, 0, 1);

                            buffer = new byte[amount[0]];
                            stream.Read(buffer, 0, amount[0]);
                            Nickname = Encoding.UTF8.GetString(buffer);

                            Console.WriteLine($"{Client.Client.RemoteEndPoint} ({oldNickname}) has changed his nickname to {Nickname}.");
                            break;
                    }
                }
            }
        }
    }
}
