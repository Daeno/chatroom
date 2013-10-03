using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;


namespace ChatClient_WPF
{
    class handleServer
    {
        TcpClient clientSocket;
        NetworkStream netstream;
        MainWindow window;
        string ctName;
        char spCh;
        public void start(TcpClient tc,string uln, char sp, MainWindow ww)
        {
            clientSocket = tc;
            ctName = uln;
            spCh = sp;
            window = ww;
            Thread ctThread = new Thread(getMessage);
            ctThread.Start();
        }

        private void getMessage()
        {
            
            while (true)
            {
                try
                {
                    //MessageBox.Show("hihi");
                    if (clientSocket.Connected)
                    {
                        
                        netstream = clientSocket.GetStream();
                        byte[] inData = new byte[clientSocket.ReceiveBufferSize];
                        netstream.Read(inData, 0, clientSocket.ReceiveBufferSize);
                        string message;

                        MsgType ty = ChatClient_WPF.MainWindow.parseMsg(ref inData);
                        message = System.Text.Encoding.ASCII.GetString(inData);
                        string[] commands = message.Split(spCh);
                        switch(ty)
                        {
                            case MsgType.S_MSG_FROM_BCGROUP:
                                int bcgp = int.Parse(commands[0]);
                                window.msg(commands[1]);
                            break;
                            case MsgType.S_ONLINE_LIST:
                                window.updateList(commands);
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return;
                }
            }
        }
    }
}
