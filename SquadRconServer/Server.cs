using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SquadRconServer.Permissions;

namespace SquadRconServer
{
    public class Server
    {
        private string _currentpath = Directory.GetCurrentDirectory();
        private Regex _IPCheck = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
        internal static readonly UTF8Encoding asen = new UTF8Encoding();
        private bool _running = true;
        internal IniParser Settings;
        internal string ListenIPAddress;
        internal int ListenPort;
        internal int TokenValidTime = 24;
        internal string CertificatePassword;
        internal string IpsAndDomains;
        internal TcpListener TCPServer;
        internal X509Certificate2 Certificate;

        public enum Codes
        {
            Login = 0,
            Unknown = 1
        }

        public Server()
        {
            bool success = LoadSettings();
            if (!success)
            {
                Logger.Log("Config failure, shutting down...");
                Thread.Sleep(1000);
                return;
            }

            GenerateSelfSignedCertificate();
            
            PermissionLoader.LoadPermissions();

            IPAddress listenip = IPAddress.Any;
            if (ListenIPAddress.ToLower() != "any")
            {
                if (!IPAddress.TryParse(ListenIPAddress, out listenip))
                {
                    Logger.Log("Failed to convert listen ip to an actual IP Address. Listening on all.");
                }
            }
            
            TCPServer = new TcpListener(listenip, ListenPort);
            TCPServer.Start();
            Logger.Log("[TCPServer] Listening for incoming connections. IP: " + listenip + " Port: " + ListenPort);
            Thread t = new Thread(ListenForIncomingConnections);
            t.IsBackground = true;
            t.Start();
        }

        internal void StopConnections()
        {
            _running = false;
            TCPServer.Stop();
        }
        
        private void GenerateSelfSignedCertificate()
        {
            if (File.Exists(_currentpath + "\\SquadRconCertificate.pfx"))
            {
                try
                {
                    var certificate = new X509Certificate2(File.ReadAllBytes(_currentpath + "\\SquadRconCertificate.pfx"), CertificatePassword);
                }
                catch (CryptographicException ex)
                {
                    var data = (ex.HResult & 0xFFFF);
                    if (data == 0x56)
                    {
                        Logger.LogError("[Certificate] Certificate password incorrect.");
                        return;
                    }
                    Logger.LogError("[Certificate] Existing Certificate has issues. Please refer to: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-?redirectedfrom=MSDN");
                    Logger.LogError("Error Code: " + data);
                    Logger.LogError("Error: " + ex);
                    return;
                } 
                Certificate = new X509Certificate2(_currentpath + "\\SquadRconCertificate.pfx", CertificatePassword);
                Logger.Log("[Certificate] Existing Certificate successfully loaded!");
                return;
            }
            Logger.Log("[Certificate] Generating self signed SSL Certificate...");
            
            if (File.Exists(_currentpath + "\\SquadRconCertificate.pfx"))
            {
                File.Delete(_currentpath + "\\SquadRconCertificate.pfx");
            }

            SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
            sanBuilder.AddDnsName("localhost");
            sanBuilder.AddDnsName(Environment.MachineName);
            
            foreach (var x in IpsAndDomains.Split(','))
            {
                if (string.IsNullOrEmpty(x)) continue;
                
                if (_IPCheck.Match(x.Trim()).Success)
                {
                    sanBuilder.AddIpAddress(IPAddress.Parse(x));
                }
                else
                {
                    sanBuilder.AddDnsName(x.Trim());
                }
            }

            X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN=SquadRconServer");

            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature , false));


                request.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(
                        new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

                request.CertificateExtensions.Add(sanBuilder.Build());

                var certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
                certificate.FriendlyName = "SquadRconServer";
                byte[] certbytes = certificate.Export(X509ContentType.Pfx, CertificatePassword);

                Certificate = new X509Certificate2(certbytes,
                    CertificatePassword, X509KeyStorageFlags.MachineKeySet);

                // Create PFX (PKCS #12) with private key
                File.WriteAllBytes(_currentpath + "\\SquadRconCertificate.pfx", certbytes);
            }
            Logger.Log("[Certificate] Complete!");
        }

        private bool LoadSettings()
        {
            try
            {
                if (!File.Exists(_currentpath + "\\Settings.ini"))
                {
                    File.Create(_currentpath + "\\Settings.ini").Dispose();
                    Settings = new IniParser(_currentpath + "\\Settings.ini");
                    Settings.AddSetting("Settings", "ListenIPAddress", "Any");
                    Settings.AddSetting("Settings", "ListenPort", "12455");
                    Settings.AddSetting("Settings", "TokenValidTime", "24");
                    Settings.AddSetting("Settings", "CertificatePassword", "ChangeMeAndDeleteCertificates");
                    Settings.AddSetting("Settings", "IpsAndDomains", "127.0.0.1,exampledomains.com");
                    Settings.Save();
                }
                Settings = new IniParser(_currentpath + "\\Settings.ini");
                ListenIPAddress = Settings.GetSetting("Settings", "ListenIPAddress");
                ListenPort = int.Parse(Settings.GetSetting("Settings", "ListenPort"));
                TokenValidTime = int.Parse(Settings.GetSetting("Settings", "TokenValidTime"));
                CertificatePassword = Settings.GetSetting("Settings", "CertificatePassword");
                IpsAndDomains = Settings.GetSetting("Settings", "IpsAndDomains");
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("[Settings] Failed to read settings! " + ex);
            }

            return false;
        }

        private void ListenForIncomingConnections()
        {
            while (_running)
            {
                Socket socket;
                try
                {
                    socket = TCPServer.AcceptSocket();
                }
                catch (SocketException)
                {
                    // ignore...
                    continue;
                }
                
                Thread t = new Thread(() =>
                {
                    HandleConnection(socket);
                });
                t.IsBackground = true;
                t.Start();
            }
        }

        private void HandleConnection(System.Net.Sockets.Socket s)
        {
            try
            {
                s.ReceiveTimeout = 0;
                s.SendTimeout = 0;
                s.NoDelay = true;
                string ipr = s.RemoteEndPoint.ToString().Split(':')[0];

                // Remove insecure protocols (SSL3, TLS 1.0, TLS 1.1)
                ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
                ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls;
                ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls11;
                // Add TLS 1.2
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;;
                NetworkStream stream = new NetworkStream(s);
                SslStream ssl = new SslStream(stream, false);
                //ssl.AuthenticateAsServer(Certificate, false, SslProtocols.Tls13 | SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls, 
                //    true);
                ssl.AuthenticateAsServer(Certificate, false, SslProtocols.Tls12, true);
                while (s.Connected)
                {
                    if (!ssl.CanRead && !stream.DataAvailable)
                    {
                        s.Close();
                        return;
                    }

                    byte[] leng = new byte[4];
                    int k2 = 0;
                    try
                    {
                        k2 = ssl.Read(leng, 0, leng.Length);
                    }
                    catch (Exception ex)
                    {
                        if (ex.ToString()
                            .Contains("An established connection was aborted by the software in your host machine"))
                        {
                            if (s.Connected)
                            {
                                s.Close();
                            }

                            return;
                        }

                        if (ex.ToString().Contains("System.NullReferenceException"))
                        {
                            if (s.Connected)
                            {
                                s.Close();
                            }

                            return;
                        }

                        Logger.Log("[Communication Error] General error. " + ipr + " " + ex);
                    }

                    Console.WriteLine(leng.Length);
                    if (BitConverter.IsLittleEndian)
                    {
                        Console.WriteLine("Reverse");
                        Array.Reverse(leng);
                        Console.WriteLine(leng.Length);
                    }

                    int upcominglength = (BitConverter.ToInt32(leng, 0));
                    Console.WriteLine(upcominglength);
                    if (upcominglength > 15000000 || upcominglength <= 0)
                    {
                        ssl.Flush();
                        s.Close();
                        return;
                    }

                    byte[] b = ByteReader(upcominglength, ssl, s);
                    if (b == null || k2 == 0)
                    {
                        ssl.Flush();
                        s.Close();
                        return;
                    }

                    string message = Encoding.UTF8.GetString(b, 0, b.Length);
                    if (string.IsNullOrEmpty(message) || !message.Contains("@"))
                    {
                        ssl.Flush();
                        s.Close();
                        return;
                    }

                    string[] split = message.Split('@');
                    if (split.Length != 2 || string.IsNullOrEmpty(split[1]) || string.IsNullOrEmpty(split[0]))
                    {
                        s.Close();
                        return;
                    }

                    Codes code = Codes.Unknown;
                    int intp = -1;
                    bool bbbb = int.TryParse(split[0], out intp);
                    if (!bbbb || intp == -1)
                    {
                        s.Close();
                        return;
                    }

                    if (!Enum.IsDefined(typeof(Codes), intp))
                    {
                        s.Close();
                        return;
                    }

                    code = (Codes) intp;

                    string[] otherdata = split[1].Split('~');
                    string bmsg = "";
                    switch (code)
                    {
                        case Codes.Login:
                        {
                            try
                            {
                                if (otherdata.Length != 4)
                                {
                                    if (s.Connected)
                                    {
                                        s.Close();
                                    }
                                    return;
                                }

                                string name = otherdata[0].Substring(0, Math.Min(otherdata[0].Length, 50));
                                string password = otherdata[1].Substring(0, Math.Min(otherdata[1].Length, 50));
                                string version = otherdata[2].Substring(0, Math.Min(otherdata[1].Length, 10));

                                if (string.IsNullOrEmpty(name)
                                    || string.IsNullOrEmpty(password)
                                    || string.IsNullOrEmpty(version))
                                {
                                    if (s.Connected)
                                    {
                                        s.Close();
                                    }
                                    return;
                                }
                                Logger.Log("[TCPServer] Incoming Authentication from " + ipr + " Name: " + name);
                                
                                if (s.Connected)
                                {
                                    byte[] messagebyte = asen.GetBytes(bmsg);
                                    byte[] intBytes = BitConverter.GetBytes(messagebyte.Length);
                                    if (BitConverter.IsLittleEndian)
                                        Array.Reverse(intBytes);
                                    s.Send(intBytes);
                                    s.Send(messagebyte);
                                    s.Close();
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError("[Authentication] Error: " + ex);
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("[HandleConnection] General Error: " + ex);
            }

        }
        
        private byte[] ByteReader(int length, SslStream stream, System.Net.Sockets.Socket socket)
        {
            if (length == 0)
            {
                return null;
            }
            
            MemoryStream ms = null;
            try
            {
                byte[] data = new byte[length];
                using (ms = new MemoryStream())
                {
                    int numBytesRead;
                    int numBytesReadsofar = 0;
                    while (socket != null && stream != null && socket.Connected)
                    {
                        numBytesRead = stream.Read(data, 0, data.Length);
                        numBytesReadsofar += numBytesRead;
                        ms.Write(data, 0, numBytesRead);
                        if (numBytesReadsofar == length)
                        {
                            break;
                        }
                    }

                    if (socket == null)
                    {
                        return null;
                    }
                    return ms.ToArray();
                }
            }
            catch
            {
                if (ms != null)
                {
                    ms.Flush();
                }

                if (stream != null)
                {
                    stream.Flush();
                }

                return null;
            }
        }
    }
}