using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace ChatServer
{
    class handleClient
    {
        TcpClient clientSocket;
        string clName;
        char spCh;

        public void startClient(TcpClient tl, string name, char cc)
        {
            this.clientSocket = tl;
            spCh = cc;
            this.clName = name;

            Thread ctThread = new Thread(clientRoutine);
            ctThread.Start();
        }
        private void clientRoutine()
        {
            
            while (true)
            {
                try
                {
                    if (clientSocket.Connected)
                    {
                        NetworkStream dataStream = this.clientSocket.GetStream();
                        BinaryReader br = new BinaryReader(dataStream);
                        //byte[] data = new byte[clientSocket.ReceiveBufferSize];
                        //dataStream.Read(data, 0, clientSocket.ReceiveBufferSize);
                        int len = br.ReadInt32();
                        byte[] data = br.ReadBytes(len);

                        switch(Program.parseMsg(ref data))
                        {
                            case MsgType.C_MSG_TO_BCGROUP:
                                string dataString = System.Text.Encoding.ASCII.GetString(data);
                            //only chatting
                                string[] commands = dataString.Split(spCh);
                                //dataString = dataString.Substring(dataString.IndexOf('\x01')+1);
                            //int len = dataString.Substring(0,dataString.IndexOf("$"));
                                //dataString = dataString.Substring(0,dataString.IndexOf('\x01'));
                                
                                Console.WriteLine("From " + clName + " - data: " + commands[0]);
                                Program.broadcastChat(commands[1], clName,int.Parse(commands[0]), true);
                            break;
                            
                        }
                    }
                    else
                    {
                        Console.WriteLine(clName + " is not connected.");
                        return;
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(clName + " closed");
                    Program.clientList.Remove(clName);
                    for (int i = 0; i < 20; i++ )
                        Program.BCGroupList[i].Remove(clName);
                    Program.broadcastChat(clName + " has left.", clName,0, false);
                    //Console.WriteLine(e.ToString());
                    return;
                }
            }
        }
    }

}
