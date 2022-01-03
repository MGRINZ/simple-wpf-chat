using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private TcpListener tcpListener;
        private TcpClient tcpClient;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Nick { get; set; }
        public IPAddress IpAddress { get; set; }
        public short Port { get; set; }

        private string message;
        public string Message {
            get => message;
            set
            {
                if (value != message)
                {
                    message = value;
                    NotifyPropertyChanged("Message");
                }
            }
        }

        private string chatBox;
        private string remoteNick;

        private event EventHandler NickNameReceived;

        public string ChatBox {
            get => chatBox;
            set
            {
                if (value != chatBox)
                {
                    chatBox = value;
                    NotifyPropertyChanged("ChatBox");
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Nick = String.Empty;
            IpAddress = IPAddress.Loopback;
            Port = 10000;
            ChatBox = String.Empty;
            Message = String.Empty;

            NickNameReceived += onNicknameReceived;

            this.DataContext = this;
        }

        private void onNicknameReceived(object sender, EventArgs e)
        {
            WriteToChatBox($"Połączono z {remoteNick} ({((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address})");
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (Nick.Trim().Length == 0)
                return;

            remoteNick = null;

            try
            {
                ConnectToServer();
            }
            catch(SocketException)
            {
                WriteToChatBox($"Nie udało się nawiązać połączenia z klientem.");
                StartServer();
            }

            NickBox.IsEnabled = false;
            IpBox.IsEnabled = false;
            PortBox.IsEnabled = false;
            ConnectButton.IsEnabled = false;
            DisconnectButton.IsEnabled = true;
        }

        private async void StartServer()
        {
            WriteToChatBox($"Uruchamianie serwera.");

            tcpListener = new TcpListener(IpAddress, Port);
            tcpListener.Start();

            WriteToChatBox($"Serwer nasłuchuje na {IpAddress}:{Port}.");

            try
            {
                tcpClient = await tcpListener.AcceptTcpClientAsync();

                WriteData(Nick);
                ReceiveData();

                SendButton.IsEnabled = true;
            }
            catch(SocketException)
            {

            }
        }

        private void ConnectToServer()
        {
            WriteToChatBox($"Próba połączenia z {IpAddress.ToString()}:{Port}.");
            tcpClient = new TcpClient(IpAddress.ToString(), Port);
            
            WriteData(Nick);
            ReceiveData();

            SendButton.IsEnabled = true;
        }

        private async void ReceiveData()
        {
            while(tcpClient.Connected)
            {
                try
                {
                    var stream = tcpClient.GetStream();

                    byte[] buffer = new byte[1024];
                    int dataReceived = await stream.ReadAsync(buffer, 0, 1024);

                    if (dataReceived == 0)
                    {
                        WriteToChatBox($"Rozłączono z {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address}");
                        Disconnect_Click(null, null);
                        break;
                    }

                    string msg = Encoding.UTF8.GetString(buffer);

                    if (remoteNick == null)
                    {
                        remoteNick = msg;
                        NickNameReceived.Invoke(this, EventArgs.Empty);
                        continue;
                    }
                    
                    WriteToChatBox(msg, remoteNick);
                }
                catch(IOException)
                {
                    if (!tcpClient.Connected)
                        return;
                }
            }
        }

        private async void WriteData(string msg)
        {
            if (tcpClient == null || !tcpClient.Connected)
                return;

            var stream = tcpClient.GetStream();

            byte[] buffer;
            buffer = Encoding.UTF8.GetBytes(msg, 0, msg.Length);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if(tcpClient != null)
                tcpClient.Close();

            if (tcpListener != null)
                tcpListener.Stop();

            NickBox.IsEnabled = true;
            IpBox.IsEnabled = true;
            PortBox.IsEnabled = true;
            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            SendButton.IsEnabled = false;
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void MessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                SendMessage();
        }

        private void SendMessage()
        {
            if (Message.Trim().Length == 0)
                return;

            WriteToChatBox(Message, Nick);
            WriteData(Message);
            Message = String.Empty;
        }

        private void WriteToChatBox(string msg, string nick = null)
        {
            DateTime date = DateTime.Now;

            if (nick != null)
                nick = " " + nick;

            ChatBox += $"[{date.ToString("dd.MM.yyyy HH:mm:ss")}]{nick}: {msg}\n";
        }

        private void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
