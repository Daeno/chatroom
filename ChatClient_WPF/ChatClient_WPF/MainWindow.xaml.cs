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
using System.Net;
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
        C_ASK_LOGIN,
        C_ADD_BCGROUP,
        C_MSG_TO_BCGROUP,
        C_ADD_FRIEND,
        C_REMOVE_FRIEND,


        //Message Types from Server to Client , 128~255
        S_REGISTER_SUCC = 128,      //  type\\\\
        S_REGISTER_FAILED,          //  type\\string cause\\\\
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
        public static char    spCh = '\x01';
        private TcpClient     clientSocket;
        private NetworkStream netstream    = default(NetworkStream);
        //string indata = null;
        private bool          fir = true;
        

        private string      account;
        private IPAddress   svrIP;
        private int         svrPort;

        private handleServer handleSvr;

        private Hashtable chatWindowMap = new Hashtable();

        private List<string> onlineList = new List<string>();
        private ObservableCollection<String> userList = new ObservableCollection<string>();
        private ObservableCollection<String> friendList;
        private ObservableCollection<String> blackList;

        private ObservableCollection<String> selectedList;


        
        
        public MainWindow()
        {
            /*
            InitializeComponent();

            updateUserListFromSvr();
            userListViewBinding();
            */
            
        }

        public MainWindow(String account, IPAddress svrIP, int svrPort, TcpClient cSocket) 
        {
            InitializeComponent();

            this.account = account;
            this.svrIP = svrIP;
            this.svrPort = svrPort;
            this.clientSocket = cSocket;


            
            checkSocketConnection();
            checkHandleServer();

            userListviewBinding();
            askForUserList(0);
        }





        public bool isConnectedToServer()
        { 
            return clientSocket.Connected; 
        }

        private Boolean checkSocketConnection()
        {
            try {
                if (clientSocket == null) {

                    clientSocket = new TcpClient();
                    clientSocket.Connect(svrIP, svrPort);
                }
                else if (!clientSocket.Connected) {
                    clientSocket.Connect(svrIP, svrPort);
                }
            }

            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
                return false;
            }

            return true;
        }

        private Boolean checkHandleServer()
        {
            try {
                if (handleSvr == null) {
                    handleSvr = new handleServer();
                    handleSvr.start(clientSocket, account, spCh, this);
                }

                else if (handleSvr.IsWorking == false){
                    handleSvr.start(clientSocket, account, spCh, this);
                }
            }
            catch (Exception ex){
                MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }


        public void askForUserList(int groupNum)
        {
            try {

                checkSocketConnection();
                checkHandleServer();

                byte[] outdata = System.Text.Encoding.ASCII.GetBytes((groupNum.ToString() + spCh + spCh + spCh).ToCharArray());
                encodeMsg(ref outdata, MsgType.C_ASK_USERLIST);
                sendBySocket(outdata);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }


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



        private void sendBySocket(byte[] data)
        {
            //debug
            checkSocketConnection();
            checkHandleServer();

            netstream = clientSocket.GetStream();
            BinaryWriter bw = new BinaryWriter(netstream);
            bw.Write(data.Length);
            bw.Write(data);
        }


        private byte[] receiveBySocket()
        {
            BinaryReader br = new BinaryReader(netstream);
            int len = br.ReadInt32();
            byte[] data = br.ReadBytes(len);
            return data;
        }


        public void getMsg(int groupNum, string message)
        {
            if (groupNum == 0) {
                msg(message);
            }
            else if (chatWindowMap.ContainsKey(groupNum)) {
                ChatWindow chatWindow = (ChatWindow)chatWindowMap[groupNum.ToString()];
                chatWindow.msg(message);
            }
            else {
            
            }
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

        /*
        public void updateList(string[] commands)
        {
            //MessageBox.Show(commands[1]);
            onlineList = new List<string>(commands);
            int len = int.Parse(onlineList[0]);
            onlineList.RemoveAt(0);
            onlineList.RemoveAt(len);
            //userList.r
            //onlineList.RemoveAt(onlineList.Count());
            updateUserListView();
            userListviewBinding();

            //MessageBox.Show(userList[0]);
        }*/


        public void updateList(List<String> list)
        {
            if (userListview.CheckAccess()) {
                userList = new ObservableCollection<string>(list.ToArray());
                userListviewBinding();
            }
            else {
                userListview.Dispatcher.Invoke(new Action(() => updateList(list)));
            }

        }




        public void updateList(string[] commands)
        {

            /*
            if (userListview.CheckAccess()) {
                userList = new ObservableCollection<string>(commands);
                int len = int.Parse(userList[0]);
                userList.RemoveAt(0);
                userList.RemoveAt(len);
                userListviewBinding();
            }
            else {
                userListview.Dispatcher.Invoke(new Action(() => updateList(commands)));
            }
             */
        }
        

        public void addToBroadcastGroup(int groupNum)
        {
            if (CheckAccess()) {
                ChatWindow chatWindow = new ChatWindow(account, svrIP, svrPort, clientSocket, groupNum);
                chatWindowMap.Add(groupNum.ToString(), chatWindow);

                chatWindow.Show();
            }
            else {
                Dispatcher.Invoke(new Action(()=> addToBroadcastGroup(groupNum)));
            }
            
        }



        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.clientSocket.Close();
            base.OnClosing(e);
        }



        //這個函式可以由 updateList完全取代，故可以放棄
        private void updateUserListView()
        {
            /*
            if (userListview.CheckAccess())
            {
                 userList = new ObservableCollection<String>(onlineList.ToArray());
            }
            else
            {
                userListview.Dispatcher.Invoke(new Action(updateUserListView));
            }*/
        }

        private void updateFriendListFromSvr() { }
        private void updateBlackListFromSvr() { }


        private void userListviewBinding()
        {
            if (userListview.CheckAccess()){
                ListView lv = userListview;
               lv.DataContext = userList;
            }
            else {
                userListview.Dispatcher.Invoke(new Action(userListviewBinding));
            }
        }






        private void changeAddButtonLook(int selectedNum)
        {
            if (selectedNum == 0) {
                userListview.Height = 291;
                buttonNewGroup.Visibility = System.Windows.Visibility.Collapsed;
            }

            else {
                userListview.Height = 260;
                
                string chat_str;

                string str1 = "跟這";
                string str2 = "個人聊天~";
                string num = selectedNum.ToString();

                if (selectedNum == 1)
                    chat_str = str1 + str2;
                else
                    chat_str = str1 + num + str2;

                buttonNewGroup.Content = chat_str;
                buttonNewGroup.Visibility = System.Windows.Visibility.Visible;

            }


        }






        /*============================================================
         *    Controls 的事件處理區
         *==============================================================*/


        private void HandleUserDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show(userListBinding.SelectedItem.ToString());
        }

        private void buttonNewGroup_Click(object sender, RoutedEventArgs e)
        {
            string data = "";
            foreach (string s in selectedList) {
                data += (s + spCh);
            }

            data += spCh;
            byte[] outdata = System.Text.Encoding.ASCII.GetBytes(data);

            encodeMsg(ref outdata, MsgType.C_ADD_BCGROUP);
            sendBySocket(outdata);
        }

        //Binding needed for userListView
        private void userListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IList selectedItems = userListview.SelectedItems;
            selectedList = new ObservableCollection<String>();

            foreach (String item in selectedItems) {
                selectedList.Add(item.ToString());
            }

            changeAddButtonLook(selectedList.Count);
        }

        private void chatText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                buttonSend.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent, buttonSend));
            }
        }

        private void userName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            //debug
            //MessageBox.Show(handleServer.handleServerServive.ToString());
            //debug
            checkSocketConnection();
            checkHandleServer();

            if (isConnectedToServer()) {
                try {
                    int groupNum = 0;

                    string outdata = groupNum.ToString() + spCh + chatText.Text.ToString() + spCh + spCh;
                    byte[] outSt = new byte[clientSocket.ReceiveBufferSize];
                    outSt = System.Text.Encoding.ASCII.GetBytes(outdata.ToCharArray());
                    encodeMsg(ref outSt, MsgType.C_MSG_TO_BCGROUP);

                    sendBySocket(outSt);
                    //netstream.Write(outSt, 0, outdata.Length);
                    //netstream.Flush();


                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                }
            }
            else {
                string indata = ">> " + userName.Text.ToString() + " says: " + chatText.Text.ToString();
                msg(indata);
            }
            chatText.Text = null;
        }

        /*
        private void userName_MouseEnter(object sender, MouseEventArgs e)
        {
            if (fir)
                userName.Text = "";
            fir = false;
        }
        */
    }
}
