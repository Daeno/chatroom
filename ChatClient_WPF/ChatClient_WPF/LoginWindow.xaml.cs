using System;
using System.Collections.Generic;
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

namespace ChatClient_WPF
{
    /// <summary>
    /// Window1.xaml 的互動邏輯
    /// </summary>
    public partial class LoginWindow : Window
    {
        private const int ipDefaultChoice = 0;
        private IPAddressChoice[] ipChoices = {new IPAddressChoice("本機伺服器(127.0.0.1)", "127.0.0.1"),
                                               new IPAddressChoice("逸安的電腦(140.112.18.207)", "140.112.18.207"),
                                               new IPAddressChoice("胤勳的電腦(140.112.18.208)", "140.112.18.208"),
                                               new IPAddressChoice("唐唐的電腦(140.112.18.209)", "140.112.18.209")};
        private string[] portChoices = { "8888" };



        private IPAddress svrIP;
        private int       svrPort;

        private String account;

        /*Socket*/
        public static char spCh = MainWindow.spCh;
        TcpClient clientSocket = new TcpClient();
        NetworkStream netstream = default(NetworkStream);


        private const string register_succ_str = "註冊成功！";
        private const string register_failed_str = "註冊失敗:(";
        private const string login_succ_str = "登入成功！";
        private const string login_failed_str = "登入失敗:((";
        private const string account_invalid_str = "帳號要用英文數字、英文開頭、15字內哦!";
        private const string password_invalid_str = "密碼要6個字以上哦";
        private const string registering_str = "註冊中...";
        private const string loginning_str = "登入中...";


        public LoginWindow()
        {
           InitializeComponent();
           initComboBoxes();
        }


        public bool connectToServer()
        { 
            return clientSocket.Connected; 
        }


        private void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
            clearResult();

            if (!checkAcctPasswd())
                return;

            try {
                showResult(registering_str);
                byte[] outdata = System.Text.Encoding.ASCII.GetBytes((account + spCh + passwordBox.Password + spCh).ToCharArray());
                sendBySocket(outdata, MsgType.C_ASK_REGISTER);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }


        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            clearResult();

            if (!checkAcctPasswd())
                return;

            try {
                showResult(loginning_str);
                byte[] outdata = System.Text.Encoding.ASCII.GetBytes((account + spCh + passwordBox.Password + spCh).ToCharArray());
                sendBySocket(outdata, MsgType.C_ASK_LOGIN);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }



        private void sendBySocket(byte[] outdata, MsgType msgType)
        {
            try {
                clientSocket.Connect(svrIP, svrPort);
                netstream = clientSocket.GetStream();
                MainWindow.encodeMsg(ref outdata, msgType);
                netstream.Write(outdata, 0, outdata.Length);
                netstream.Flush();
                handleServer hs = new handleServer();
                hs.start(clientSocket, account, spCh, this);
            }
            catch (Exception Ex) {
                MessageBox.Show(Ex.ToString());
            }
        }



        public void registerSucc()
        {
            showResult(register_succ_str);
        }

        public void registerFalied(String cause)
        {
            showResult(register_failed_str + '\n' + cause);
        }

        public void loginSucc()
        {
            showResult(login_succ_str);
            MainWindow mainWindow = new MainWindow(account, svrIP, svrPort);
            mainWindow.Show();
            Close();
        }

        public void loginFailed(String cause)
        {
            showResult(login_failed_str + '\n' + cause);
        }





        public void showResult(String result)
        {
            textBlockResult.Text = result;
        }
        public void clearResult()
        {
            showResult("");
        }


        
        private void initComboBoxes()
        {

            //ip combobox
            ComboBox ipChooser = cboxIP;
            List<string> ips = new List<string>(4);
            foreach (IPAddressChoice ip in ipChoices) {
                ips.Add(ip.Text);
            }

            ipChooser.DataContext = ips;
            ipChooser.SelectedIndex = ipDefaultChoice;
            


            //port combobox
            ComboBox portChooser = cboxPort;
            portChooser.DataContext = portChoices;

            cboxPort.SelectedIndex = 0;
        }




        private Boolean checkAcctPasswd()
        {
            if (!isValidAccount(account)) {
                showResult(account_invalid_str);
                return false;
            }

            if (!isValidPassword(passwordBox)) {
                showResult(password_invalid_str);
                return false;
            }
            return true;
        }














        private void cboxIP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            svrIP = ipChoices[cboxIP.SelectedIndex].IpAddr;
        }

        private void cboxPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            svrPort = Int32.Parse(portChoices[cboxPort.SelectedIndex]);
        }

        private void textBoxAccount_TextChanged(object sender, TextChangedEventArgs e)
        {
            account = textBoxAccount.Text;
        }










        //大小寫英文開頭，只包含英文數字，長度maxLength以內
        public  static Boolean isValidAccount(String account)
        {
            if (account == null)
                return false;

            const int maxLenth = 15;
            char head = account[0];

            //太長或太短
            if (account.Length >= maxLenth || account.Length == 0) {
                return false;
            }

            //開頭不是英文
            if (!((head >= 65 && head <= 90) || (head >= 97 && head <= 122))) {
                return false;
            }

            //有任何字不是英文或數字
            for (int i = 1; i < account.Length; i++) {
                if (!isLetterOrNum(account[i]))
                    return false;
            }

            return true;
        }

        public  static Boolean isLetterOrNum(char c)
        {
            //number
            if (c >= 48 && c <= 57)
                return true;
            if (c >= 65 && c <= 90)
                return true;
            if (c >= 97 && c <= 122)
                return true;
            return false;
        }

        public static Boolean isValidPassword(PasswordBox passwordBox)
        {
            if (passwordBox.Password.Length < 6)
                return false;

            return true;
        }











        //把IP跟combobox選項打包在一起
        private class IPAddressChoice
        {
            private string text;
            private IPAddress ip;

            public IPAddressChoice(string t, string addr)
            {
                text = t;
                ip = IPAddress.Parse(addr);
            }

            public String Text { get { return text; } }
            public IPAddress IpAddr { get { return ip;   } }
        }



        //按視窗，製造可以拖曳移動的效果
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        //按叉叉
        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }




    }
}
