﻿using System;
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
            C_ASK_USERLIST,
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
        //改這個值，true->127.0.0.1 ; false->"140.112.18.XXX"
        private static readonly bool isLocalhost = true;

        private static readonly string localhostIP_str = "127.0.0.1";
        private static readonly int svrPort = 8888;

        private static IPAddress svrIP;
        private static IPEndPoint svrEndPoint;
        private static IPHostEntry svrHostEntry;
 

        public const char spCh = '\x01';
        public static Hashtable clientList = new Hashtable();
        public static List<string>[] BCGroupList = new List<string>[20];

        public  

        static void Main(string[] args)
        {
            //byte[] test =System.Text.Encoding.ASCII.GetBytes("\x00\x02\x03");
            //parseMsg(ref test);
            //encodeMsg(ref test,MsgType.S_REGISTER_RESULT);
            
            setupIPandEP(isLocalhost);

            //Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //serverSocket.Bind(serverEP);

            TcpListener serverSocket = new TcpListener(svrIP, svrPort);
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
                    
                    MsgType msgType = parseMsg(ref byteFrom);

                    if (msgType == MsgType.C_ASK_REGISTER)
                    {
                        clientName = System.Text.Encoding.ASCII.GetString(byteFrom);
                        clientName = clientName.Substring(0, clientName.IndexOf(spCh));

                        clientList.Add(clientName, clientSocket);
                        BCGroupList[0].Add(clientName);

                        broadcastChat(clientName + " has joined the chatroom.", clientName,0, false);
                        broadcastList(0);
                        Console.WriteLine(clientName + " has joined.");
                        handleClient cc = new handleClient();
                        cc.startClient(clientSocket, clientName, spCh);
                    }
                    
                }
        }


        private static void setupIPandEP(bool isLocalhost)
        {
            if (isLocalhost) {
                svrIP = IPAddress.Parse(localhostIP_str);
                svrEndPoint = new IPEndPoint(svrIP, svrPort);
            }
            else {
                IPAddress[] ipList;

                string hostname = Dns.GetHostName();
                svrHostEntry = Dns.GetHostEntry(hostname);
                ipList = svrHostEntry.AddressList;

                foreach (IPAddress ip in ipList) {
                    IPEndPoint ep = new IPEndPoint(ip, svrPort);

                    if (ip.AddressFamily == AddressFamily.InterNetwork) {
                        svrEndPoint = ep;
                        svrIP = ip;
                    }
                }
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

        //send the usernames in gpn'th broadcast group to everyone in the group
        public static void broadcastList(int gpn)
        {
            int len = BCGroupList[gpn].Count();
            string data = len.ToString();

            foreach (string item in BCGroupList[0])
            {
                data += (string)(spCh + item);
            }
            data += (spCh);
            byte[] outdata = System.Text.Encoding.ASCII.GetBytes(data);

            encodeMsg(ref outdata, MsgType.S_ONLINE_LIST);
            broadcastGP(gpn, outdata, MsgType.S_ONLINE_LIST);
        }

        //user "usrName" send "msg"(chatting) to the gpn'th broadcast group
        public static void broadcastChat(string msg,string usrName, int gpn, bool showName)
        {
            
            byte[] sendingData = new byte[65536];
            if (showName)
                {
                    sendingData = System.Text.Encoding.ASCII.GetBytes((gpn.ToString() + spCh + ">> " + usrName + " says: " + msg + spCh).ToCharArray());
                }
                else
                {
                    sendingData = System.Text.Encoding.ASCII.GetBytes((gpn.ToString() + spCh + ">> " + msg + spCh).ToCharArray());
                }
            encodeMsg(ref sendingData, MsgType.S_MSG_FROM_BCGROUP);
            broadcastGP(0, sendingData , MsgType.S_MSG_FROM_BCGROUP);
        }


        //send data(data) of MsgType(msgType) to the chosen broadcast group (the gpn'th one)
        public static void broadcastGP(int gpn,byte[] data,MsgType msgType)
        {
            
            foreach(string item in BCGroupList[gpn])
            {
                TcpClient sendingClient = (TcpClient)clientList[item];
                NetworkStream sendingStream = sendingClient.GetStream();
                
                sendingStream.Write(data, 0, data.Length);
                sendingStream.Flush();

                Console.WriteLine("Broadcasting " + msgType.ToString() + " to Group " + gpn.ToString() + " user:" + item);
                //sendingStream.Close(0);
            }
        }
        
    }

   
}
