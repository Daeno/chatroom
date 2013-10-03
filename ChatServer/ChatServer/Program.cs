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
    public enum MsgType :byte
        { 
        
        //Message Types from Client to Server, 0~127
            C_ASK_REGISTER = 0,
            C_ASK_USERNAME_EXIST,
            C_ASK_ONLINE,
            C_ADD_BCGROUP,
            C_MSG_TO_BCGROUP,
            C_ADD_FRIEND,
            C_REMOVE_FRIEND,


        //Message Types from Server to Client , 128~255
            S_REGISTER_RESULT = 128,
            S_LOGIN_FAILED,
            S_LOGIN_SUCC,
            S_ADD_TO_BCGROUP,
            S_MSG_FROM_BCGROUP,
            S_ONLINE_LIST,
            S_ADD_FRIEND,
            S_REMOVE_FRIEND
        };

        

    class Program
    {
        const char spCh = '\x01';
        public static Hashtable clientList = new Hashtable();
        public static List<string>[] BCGroupList = new List<string>[20];
        static void Main(string[] args)
        {
            byte[] test =System.Text.Encoding.ASCII.GetBytes("\x00\x02\x03");
            parseMsg(ref test);
            encodeMsg(ref test,MsgType.S_REGISTER_RESULT);
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
        public static MsgType parseMsg(ref byte[] indata)
        {
            MsgType type;
            type = (MsgType)indata[0];
            byte[] temp = new byte[indata.Length];
            //for (int i = 0; i < indata.Length - 1; i++)
            //    temp[i] = indata[i + 1];
            Array.Copy(indata,1,temp,0,indata.Length - 1);
            indata = temp;
            return type;
        }
        public static void encodeMsg(ref byte[] indata, MsgType type)
        {
            byte[] output = new byte[indata.Length + 1];
            output[0] = (byte)type;
            Array.Copy(indata, 0, output, 1, indata.Length);
            indata = output;
        }


        public static void broadcastChat(string msg,string usrName, bool showName)
        {
            
            byte[] sendingData = new byte[65536];
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
