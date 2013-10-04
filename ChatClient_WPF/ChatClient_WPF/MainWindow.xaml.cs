using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;

namespace ChatClient_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public enum MsgType : byte
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
    public partial class MainWindow : Window
    {
        char spCh = '\x01';
        TcpClient clientSocket = new TcpClient();
        NetworkStream netstream = default(NetworkStream);
        //string indata = null;
        bool fir = true;
        List<string> onlineList = new List<string>();
        ObservableCollection<String> userList = new ObservableCollection<string>();
        ObservableCollection<String> friendList;
        ObservableCollection<String> blackList;

        
        public MainWindow()
        {
            InitializeComponent();

            updateUserListFromSvr();
            userListViewBinding();
        }
        public bool connectToServer()
        { return clientSocket.Connected; }

        public static MsgType parseMsg(ref byte[] indata)
        {
            MsgType type;
            type = (MsgType)indata[0];
            byte[] temp = new byte[indata.Length];
            //for (int i = 0; i < indata.Length - 1; i++)
            //    temp[i] = indata[i + 1];
            Array.Copy(indata, 1, temp, 0, indata.Length - 1);
            indata = temp;
            return type;
        }
        public static void encodeMsg(ref byte[] indata, MsgType type)
        {
            byte[] output = new byte[indata.Length + 1];
            output[0] = (byte)type;
            Array.Copy(indata, 0, output, 1, indata.Length );
            indata = output;
        }
        private void userName_MouseEnter(object sender, MouseEventArgs e)
        {
            if(fir)
                userName.Text = "";
            fir = false;
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                userName.IsReadOnly = true;
                clientSocket.Connect("140.112.18.208", 8888);
                netstream = clientSocket.GetStream();
                byte[] outdata = System.Text.Encoding.Unicode.GetBytes((userName.Text+ spCh).ToCharArray());
                encodeMsg(ref outdata, MsgType.C_ASK_REGISTER);
                netstream.Write(outdata, 0, outdata.Length);
                netstream.Flush();
                handleServer hs = new handleServer();
                hs.start(clientSocket, userName.Text.ToString(), spCh,this);
            }
            catch
            {
                MessageBox.Show("Something's Wrong!");
            }
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            if (connectToServer())
            {
                try
                {
                    string outdata = "0" + spCh + chatText.Text.ToString() + spCh + spCh;
                    byte[] outSt = new byte[clientSocket.ReceiveBufferSize];
                    outSt = System.Text.Encoding.Unicode.GetBytes(outdata.ToCharArray());
                    encodeMsg(ref outSt, MsgType.C_MSG_TO_BCGROUP);

                    BinaryWriter bw = new BinaryWriter(netstream);
                    //netstream.Write(outSt, 0, outdata.Length);
                    //netstream.Flush();
                    bw.Write(outSt.Length);
                    bw.Write(outSt);
                }
                catch
                {
                    MessageBox.Show("Stupid Daeno!");
                }
            }
            else
            {
                string indata = ">> "+ userName.Text.ToString() + " says: " + chatText.Text.ToString();
                msg(indata);
            }
            chatText.Text = null;


        }

        public void msg(string s)
        {
            if(chatDisplay.Dispatcher.CheckAccess())
            {
                chatDisplay.Text = chatDisplay.Text + Environment.NewLine + s;
                chatDisplay.ScrollToEnd();
            }
            else
            {
                chatDisplay.Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(msg),s);
            }
        }
        public void updateList(string[] commands)
        {
            //MessageBox.Show(commands[1]);
            onlineList = new List<string>(commands);
            int len = int.Parse(onlineList[0]);
            onlineList.RemoveAt(0);
            onlineList.RemoveAt(len);
            //userList.r
            //onlineList.RemoveAt(onlineList.Count());
            updateUserListFromSvr();
            userListViewBinding();

            //MessageBox.Show(userList[0]);
        }
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.clientSocket.Close();
            base.OnClosing(e);
        }

        private void chatText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                buttonSend.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent,buttonSend));
            }
        }

        private void userName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }





       


        private void updateUserListFromSvr()
        {
            

            //test
            if (userListBinding.CheckAccess())
            {

               
                 userList = new ObservableCollection<String>(onlineList.ToArray());
            }
            else
            {
                userListBinding.Dispatcher.Invoke(new Action(updateUserListFromSvr));
            }

            //userListBox.



           

           // userList = new ObservableCollection<String>(onlineList.ToArray());
        }
        private void updateFriendListFromSvr() { }
        private void updateBlackListFromSvr() { }


        private void userListViewBinding()
        {
            if (userListBinding.CheckAccess())
            {

               ListView lv = userListBinding;
               lv.DataContext = userList;
            }
            else
            {
                userListBinding.Dispatcher.Invoke(new Action(userListViewBinding));
            }
        }



        //Binding needed for userListView
        private void userListBinding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }


        private void HandleUserDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show(userListBinding.SelectedItem.ToString());
        }





    }
}
