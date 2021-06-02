using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
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
using SquadRconLibrary;
using SquadRconLibrary.Compression;
using SquadRconServer;

namespace SquadRconClient
{
    
    public enum Codes
    {
        Login = 0,
        Unknown = 1
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static readonly UTF8Encoding asen = new UTF8Encoding();
        internal ServerChooser Chooser;
        
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private static bool ValidateCert(object sender, X509Certificate certificate, 
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors && chain.ChainStatus.Length == 1)
            {
                if (chain.ChainStatus[0].StatusInformation.Contains(
                    "A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider."))
                {
                    return true;
                }
            }
            return false;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string ip = IPAddressBox.Text;
            string port = PortBox.Text;
            string username = UserNameBox.Text;
            string password = PasswordBox.Password;
            int PortNum;

            if (string.IsNullOrEmpty(ip))
            {
                AutoClosingMessageBox.Show("IP Box MUST have a value.", "Error", 3000, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(port))
            {
                AutoClosingMessageBox.Show("Port Box MUST have a value.", "Error", 3000, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(username))
            {
                AutoClosingMessageBox.Show("UserName Box MUST have a value.", "Error", 3000, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                AutoClosingMessageBox.Show("Password Box MUST have a value.", "Error", 3000, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool isdomain = Uri.CheckHostName(ip) == UriHostNameType.Dns;
            if (isdomain)
            {
                var addresses = Dns.GetHostAddresses(ip);
                if (addresses.Length > 0)
                {
                    ip = addresses[0].ToString();
                }
            }
            Match match = Regex.Match(ip, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");

            IPAddress addr;
            if (!IPAddress.TryParse(ip, out addr) || !match.Success)
            {
                AutoClosingMessageBox.Show("The IP input is not an IP address.", "Error", 3000, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(port, out PortNum))
            {
                AutoClosingMessageBox.Show("The Port input is not a number.", "Error", 3000, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Chooser = new ServerChooser(addr, PortNum, username, password);
            Chooser.Show();
        }
    }
}