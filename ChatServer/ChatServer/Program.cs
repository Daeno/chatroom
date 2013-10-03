using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ChatServer
{
    
    class Program
    {
        const char spCh = '\x01';
        public static Hashtable clientList = new Hashtable();
        public static List<string>[] BCGroupList = new List<string>[20];
        static void Main(string[] args)
        {
            string hostname = Dns.GetHostName();
            IPAddress serverIP = Dns.Resolve(hostname).AddressList[0];
            //Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //IPEndPoint serverEP = new IPEndPoint(serverIP, 8888);
            //serverSocket.Bind(serverEP);
            TcpListener serverSocket = new TcpListener(8888);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;
            //serverSocket.Listen(10);
            Console.WriteLine("Chat server started...");
            serverSocket.Start();
            for (int i = 0; i < 20; i++) BCGroupList[i] = new List<string>();

                while (true)
                {
                    counter += 1;
                    clientSocket = serverSocket.AcceptTcpClient();
                    byte[] byteFrom = new byte[clientSocket.ReceiveBufferSize];
                    string clientName = null;

                    NetworkStream incomingStream = clientSocket.GetStream();
                    incomingStream.Read(byteFrom, 0, clientSocket.SendBufferSize);
                    clientName = System.Text.Encoding.ASCII.GetString(byteFrom);
                    clientName = clientName.Substring(0, clientName.IndexOf(spCh));

                    clientList.Add(clientName, clientSocket);
                    BCGroupList[0].Add(clientName);

                    broadcastChat(clientName + " has joined the chatroom.", clientName, false);

                    Console.WriteLine(clientName + " has joined.");
                    handleClient cc = new handleClient();
                    cc.startClient(clientSocket, clientName);

                }
        }

        public static void broadcastChat(string msg,string usrName, bool showName)
        {
            
            byte[] sendingData = new byte[12000];
            if (showName)
                {
                    sendingData = System.Text.Encoding.ASCII.GetBytes((">> " + usrName + " says: " + msg + spCh).ToCharArray());
                }
                else
                {
                    sendingData = System.Text.Encoding.ASCII.GetBytes((">> "+ msg + spCh).ToCharArray());
                }
            broadcastGP(0, sendingData);
        }
        public static void broadcastGP(int gpn,byte[] data)
        {
            
            foreach(string item in BCGroupList[gpn])
            {
                TcpClient sendingClient = (TcpClient)clientList[item];
                NetworkStream sendingStream = sendingClient.GetStream();
                sendingStream.Write(data, 0, data.Length);
                sendingStream.Flush();

                Console.WriteLine("Broadcasting to Group " + gpn.ToString());
            }
        }
        
    }

   
}
