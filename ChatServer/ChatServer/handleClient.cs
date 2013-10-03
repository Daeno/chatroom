using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;

namespace ChatServer
{
    class handleClient
    {
        TcpClient clientSocket;
        string clName;

        public void startClient(TcpClient tl, string name)
        {
            this.clientSocket = tl;
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
                        byte[] data = new byte[clientSocket.ReceiveBufferSize];
                        dataStream.Read(data, 0, clientSocket.ReceiveBufferSize);


                        string dataString = System.Text.Encoding.ASCII.GetString(data);
                        //only chatting
                        dataString = dataString.Substring(dataString.IndexOf('\x01')+1);
                        //int len = dataString.Substring(0,dataString.IndexOf("$"));
                        dataString = dataString.Substring(0,dataString.IndexOf('\x01'));
                        Console.WriteLine("From " + clName + " - data: " + dataString);
                        Program.broadcastChat(dataString, clName, true);
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
                    Console.WriteLine(e.ToString());
                    return;
                }
            }
        }
    }

}
