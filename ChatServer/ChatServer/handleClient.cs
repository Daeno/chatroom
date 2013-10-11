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
        char spCh;

        public void startClient(TcpClient tl, string name, char spCh)
        {
            this.clientSocket = tl;
            this.spCh = spCh;
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

                        switch(Program.parseMsg(ref data))
                        {
                            case MsgType.C_ASK_USERLIST:
                                //debug
                                Console.WriteLine("get C_ASK_USERLIST");
                                
                                
                                Program.broadcastList(0);

                                break;

                            case MsgType.C_MSG_TO_BCGROUP:
                                string dataString = System.Text.Encoding.ASCII.GetString(data);


                            //only chatting
                                string[] commands = dataString.Split(spCh);
                                //dataString = dataString.Substring(dataString.IndexOf('\x01')+1);
                            //int len = dataString.Substring(0,dataString.IndexOf("$"));
                                //dataString = dataString.Substring(0,dataString.IndexOf('\x01'));

                                int groupNum = int.Parse(commands[0]);
                                string message = commands[1];
                                

                                Console.WriteLine("From " + clName + " - data: " + commands[0]);
                                Program.broadcastChat(message, clName, groupNum, true);
                                break;
                            case MsgType.C_ADD_BCGROUP:
                                dataString = System.Text.Encoding.ASCII.GetString(data);

                                commands = dataString.Split(spCh);
                                List<string> userList = new List<string>(commands);
                                userList.RemoveAt(userList.Count - 1);

                                Program.addToBroadcastList(userList);

                                Console.WriteLine("get C_ADD_BDGROUP");
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
                    Console.WriteLine("error: " + e.ToString());

                    Console.WriteLine(clName + " closed");
                    Program.clientList.Remove(clName);
                    for (int i = 0; i < Program.BCGroupCount; i++){  //(int i = 0; i < 20; i++ )
                        if (Program.BCGroupMap.ContainsKey(i))
                            ((List<string>)Program.BCGroupMap[i]).Remove(clName);
                    }
                    Program.broadcastChat(clName + " has left.", clName,0, false);
                    return;
                }
            }
        }
    }

}
