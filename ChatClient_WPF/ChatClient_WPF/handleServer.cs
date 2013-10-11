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
    public class handleServer
    {
        //these are all for debugging ////////
        public static int handleServerCount = 0;
        public int handleServerNum = 0;
        public static int handleServerServive = 0;
        ////////////////////////////////////////


        TcpClient clientSocket;
        NetworkStream netstream;
        Window window;
        string ctName;  //帳號
        char spCh;   //切割字符
        private Boolean isWorking;

        public void start(TcpClient tc,string uln, char sp, Window ww)
        {
            clientSocket = tc;
            ctName = uln;
            spCh = sp;
            window = ww;
            Thread ctThread = new Thread(getMessage);
            ctThread.Start();
            isWorking = true;

            //debug
            //MessageBox.Show("New handleServer !" + handleServerCount.ToString());
            handleServerNum = handleServerCount;
            handleServerCount++;
            handleServerServive++;
        }

        private void getMessage()
        {
            bool close = false;

            while (!close)
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
                            case MsgType.S_ADD_TO_BCGROUP:
                                int groupNum = int.Parse(commands[0]);

                                if (window.GetType().Name == "MainWindow")
                                    ((MainWindow)window).addToBroadcastGroup(groupNum);
                                break;

                            case MsgType.S_MSG_FROM_BCGROUP:
                                groupNum = int.Parse(commands[0]);
                                string msg = commands[1];
                                sendMsgToBCGroup(groupNum, msg);
                                // ((MainWindow)window).msg(commands[0], commands[1]);
                                break;

                            case MsgType.S_ONLINE_LIST:
                                //((MainWindow)window).updateList(commands);
                                groupNum = int.Parse(commands[0]);

                                setListOfBCGroup(groupNum, commands);
                                break;

                            case MsgType.S_REGISTER_SUCC:
                                close = true;
                                ((LoginWindow)window).Dispatcher.Invoke(new Action(((LoginWindow)window).registerSucc));
                                break;

                            case MsgType.S_REGISTER_FAILED:
                                close = true;
                                string cause = commands[0];
                                ((LoginWindow)window).Dispatcher.Invoke(new Action(() => ((LoginWindow)window).registerFailed(cause)));
                                break;

                            case MsgType.S_LOGIN_SUCC:
                                close = true;
                                ((LoginWindow)window).Dispatcher.Invoke(new Action(((LoginWindow)window).loginSucc));
                                break;
                            case MsgType.S_LOGIN_FAILED:
                                close = true;
                                cause = commands[0];
                                ((LoginWindow)window).Dispatcher.Invoke(new Action(() => ((LoginWindow)window).loginFailed(cause)));
                                break;


                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return;
                } // end catch

                handleServerServive--;
            }//end while        

            isWorking = false;
        }


        private void sendMsgToBCGroup(int groupNum, string message)
        {
            Boolean sentToWindow = false;

            if (groupNum == 0) {
                if (window.GetType().Name == "MainWindow") {
                    ((MainWindow)window).msg(message);
                    return;
                }
            }
            else {
                if (window.GetType().Name == "ChatWindow"){
                    if (((ChatWindow)window).GROUPNUM == groupNum){
                        ((ChatWindow)window).msg(message);
                        sentToWindow = true;
                        return;
                    }
                }
            }

            if (!sentToWindow) {
                MessageBox.Show("Got message from group: " + groupNum.ToString() + " , but I'm not in the group!");
            }
        }

        private void setListOfBCGroup(int groupNum, string[] commands)
        {
            Boolean sentToWindow = false;

            List<string> userList = new List<string>(commands);
            int len = int.Parse(userList[1]);

            userList.RemoveAt(0);
            userList.RemoveAt(0);
            userList.RemoveAt(len);

            //debug
            /*string list = "";
            foreach (string i in userList) {
                list += (userList.IndexOf(i).ToString() + " : " + i.ToString() + '\n');
            }
            MessageBox.Show(list);*/

            //MessageBox.Show("add list groupNum = " + groupNum.ToString());

            if (groupNum == 0) {
                if (window.GetType().Name == "MainWindow") {
                    ((MainWindow)window).updateList(userList);

                    return;
                }
            }
            else {
                if (window.GetType().Name == "ChatWindow") {
                    if (((ChatWindow)window).GROUPNUM == groupNum) {
                        //((ChatWindow)window).updateList(userList);
                        sentToWindow = true;
                        return;
                    }
                }
            }
            if (!sentToWindow) {
                MessageBox.Show("Got userlist from group: " + groupNum.ToString() + " , but I'm not in the group!");
            }
        }


        public void changeWindow(Window newWindow)
        {
            window = newWindow;
        }

        public Boolean IsWorking
        {
            get { return isWorking; }
        }
    }
}
