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
    public partial class Window1 : Window
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
        public static const char spCh = MainWindow.spCh;
        TcpClient clientSocket = new TcpClient();
        NetworkStream netstream = default(NetworkStream);


        public Window1()
        {
           InitializeComponent();
           initComboBoxes();
        }


        public bool connectToServer()
        { 
            return clientSocket.Connected; 
        }


        /*
        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            try {
                userName.IsReadOnly = true;
                clientSocket.Connect("127.0.0.1", 8888);
                netstream = clientSocket.GetStream();
                byte[] outdata = System.Text.Encoding.ASCII.GetBytes((userName.Text + spCh).ToCharArray());
                encodeMsg(ref outdata, MsgType.C_ASK_REGISTER);
                netstream.Write(outdata, 0, outdata.Length);
                netstream.Flush();
                handleServer hs = new handleServer();
                hs.start(clientSocket, userName.Text.ToString(), spCh, this);
            }
            catch {
                MessageBox.Show("Something's Wrong!");
            }
        }
        */

        private void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
            try {
                textBoxAccount.IsReadOnly = true;
                byte[] outdata = System.Text.Encoding.ASCII.GetBytes((account + spCh + passwordBox.Password + spCh).ToCharArray());
                sendBySocket(outdata, MsgType.C_ASK_REGISTER);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }


        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
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




        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }




    }
}
