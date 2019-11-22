using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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
        
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                TcpClient TCPClient = new TcpClient();
                // Remove insecure protocols (SSL3, TLS 1.0, TLS 1.1)
                ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
                ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls;
                ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls11;
                // Add TLS 1.2
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                TCPClient.NoDelay = true;
                var result = TCPClient.BeginConnect("127.0.0.1", 12455, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10));
                if (success && TCPClient.Connected)
                {
                    NetworkStream stream = TCPClient.GetStream();
                    var ssl = new SslStream(stream, false,
                        new RemoteCertificateValidationCallback(ValidateCert), null);
                    Console.WriteLine("wow");
                    ssl.AuthenticateAsClient("127.0.0.1");
                    Console.WriteLine("wow22");

                    string msg = (int) Codes.Login + Constants.MainSeparator
                                                   + "DreTaX"
                                                   + Constants.AssistantSeparator
                                                   + "test"
                                                   + Constants.AssistantSeparator
                                                   + "1.0";
                    byte[] ba = asen.GetBytes(msg);
                    ba = LZ4Compresser.Compress(ba);
                    
                    byte[] intBytes = BitConverter.GetBytes(ba.Length);
                    Console.WriteLine(intBytes.Length);
                    
                    ssl.Write(intBytes, 0, intBytes.Length);
                    ssl.Flush();
                    ssl.Write(ba, 0, ba.Length);
                    //TCPClient.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
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
        
    }
}