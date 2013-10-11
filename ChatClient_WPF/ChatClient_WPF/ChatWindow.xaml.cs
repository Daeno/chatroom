using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ChatClient_WPF
{
    /// <summary>
    /// Window1.xaml 的互動邏輯
    /// </summary>
    public partial class ChatWindow : Window
    {
        private int groupNum;
        public int GROUPNUM { get { return groupNum; } }
        private ObservableCollection<String> userList;


        public static char spCh = '\x01';
        

        private TcpClient clientSocket;
        private handleServer handleSvr;  //各聊天室窗獨立，目的：避免傳輸大檔案或大量訊息輸入時各視窗塞車
        private NetworkStream netstream = default(NetworkStream);

        private string account;
        private IPAddress svrIP;
        private int svrPort;

        private List<string> onlineList = new List<string>();


        public ChatWindow()
        {
            InitializeComponent();
        }

        public ChatWindow(String account, IPAddress svrIP, int svrPort, TcpClient cSocket, int groupNum)
        {
            InitializeComponent();

            this.account = account;
            this.svrIP = svrIP;
            this.svrPort = svrPort;
            this.clientSocket = cSocket;
            this.groupNum = groupNum;

            checkSocketConnection();
            checkHandleServer();

            userListviewBinding();
            askForUserList();
        }



        public void showMessage(string userName, string message)
        {
            //TODO

            //希望能改為此模式，傳送使用者名稱和訊息內容。取代msg。
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

                else if (handleSvr.IsWorking == false) {
                    handleSvr.start(clientSocket, account, spCh, this);
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }

        private void askForUserList()
        {
            try {
                checkSocketConnection();
                checkHandleServer();

                byte[] outdata = System.Text.Encoding.ASCII.GetBytes((GROUPNUM.ToString() + spCh + spCh + spCh).ToCharArray());
                MainWindow.encodeMsg(ref outdata, MsgType.C_ASK_USERLIST);
                sendBySocket(outdata);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private void sendBySocket(byte[] data)
        {
            checkSocketConnection();
            checkHandleServer();

            netstream = clientSocket.GetStream();
            BinaryWriter bw = new BinaryWriter(netstream);
            bw.Write(data.Length);
            bw.Write(data);
        }

        public void updateList(string[] commands)
        {
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
        }

        private void userListviewBinding()
        {
            if (userListview.CheckAccess()) {
                ListView lv = userListview;
                lv.DataContext = userList;
            }
            else {
                userListview.Dispatcher.Invoke(new Action(userListviewBinding));
            }
        }


        /*============================================================
         *    Controls 的事件處理區
         *==============================================================*/

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            checkSocketConnection();
            checkHandleServer();

            if (clientSocket.Connected) {
                try {
                    string outdata = groupNum.ToString() + spCh + textBoxMsg.Text.ToString() + spCh + spCh;
                    byte[] outSt = new byte[clientSocket.ReceiveBufferSize];
                    outSt = System.Text.Encoding.ASCII.GetBytes(outdata.ToCharArray());
                    MainWindow.encodeMsg(ref outSt, MsgType.C_MSG_TO_BCGROUP);

                    sendBySocket(outSt);
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                }
            }
            else {
                string indata = ">> " + account + " says: " + textBoxMsg.Text.ToString();
                msg(indata);
            }
            textBoxMsg.Text = "";
        }

        public void msg(string s)
        {
            if (textBoxChatDisp.Dispatcher.CheckAccess()) {
                textBoxChatDisp.Text = textBoxChatDisp.Text + Environment.NewLine + s;
                textBoxChatDisp.ScrollToEnd();
            }
            else {
                textBoxChatDisp.Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(msg), s);
            }
        }

    }
}
