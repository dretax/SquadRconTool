using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using SquadRconLibrary;
using SquadRconLibrary.Compression;
using SquadRconLibrary.JsonSerializable;
using SquadRconServer.Exceptions;
using SquadRconServer.Permissions;
using SquadRconServer.RCONHandler;
using SquadRconServer.ResponseProcessers;
using SquadRconServer.ServerContainer;
using SquadRconServer.Tokens;

namespace SquadRconServer
{
    internal class Server
    {
        private string _currentpath = Directory.GetCurrentDirectory();
        private bool _running = true;
        internal IniParser Settings;
        internal string ListenIPAddress;
        internal int ListenPort;
        internal string CertificatePassword;
        internal string IpsAndDomains;
        internal TcpListener TCPServer;
        internal X509Certificate2 Certificate;
        internal Dictionary<string, RconServerConnector> ValidServers = new Dictionary<string, RconServerConnector>();
        
        internal static string RegistrationSalt;
        internal static int TokenValidTime = 24;
        internal static readonly UTF8Encoding asen = new UTF8Encoding();
        internal static readonly Regex IPCheck = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
        internal static readonly Regex DomainCheck = new Regex(@"(http[s]?:\/\/|[a-z]*\.[a-z]{3}\.[a-z]{2})([a-z]*\.[a-z]{3})|([a-z]*\.[a-z]*\.[a-z]{3}\.[a-z]{2})|([a-z]+\.[a-z]{3})");

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
            
            SquadServerLoader.LoadServers();
            PermissionLoader.LoadPermissions();
            ValidateServers();

            IPAddress listenip = IPAddress.Any;
            if (ListenIPAddress.ToLower() != "any")
            {
                if (!IPAddress.TryParse(ListenIPAddress, out listenip))
                {
                    Logger.Log("Failed to convert listen ip to an actual IP Address. Listening on all.");
                }
            }
            
            // Remove insecure protocols (SSL3, TLS 1.0, TLS 1.1)
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls;
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls11;
            // Add TLS 1.2, and 1.3
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls13;
            
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

        /// <summary>
        /// Tries to connect to every single configured server.
        /// If one of them fails the Server will remove them from the list
        /// so the client will not see It upon authorization.
        /// </summary>
        private void ValidateServers()
        {
            Logger.Log("Validating configured servers... Please stand by.");
            List<string> invalidservers = new List<string>(SquadServerLoader.AllServers.Values.Count);
            foreach (var x in SquadServerLoader.AllServers.Values)
            {
                RconServerConnector connector = new RconServerConnector();
                ServerConnectionInfo info = new ServerConnectionInfo(x.DomainIPContainer.IP, x.RconPort, x.QueryPort, x.RconPassword);
                if (!connector.Connect(info))
                {
                    invalidservers.Add(x.ServerNickName);
                    Logger.Log("[ServerValidation] " + x.ServerNickName + " is invalid. Unable to connect, removing.");
                }
                else
                {
                    Logger.Log("[ServerValidation] " + x.ServerNickName + " is valid. Connection established.");
                    ValidServers[x.ServerNickName] = connector;
                }
            }

            foreach (var x in invalidservers.Where(x => SquadServerLoader.AllServers.ContainsKey(x)))
            {
                SquadServerLoader.AllServers.Remove(x);
            }
        }
        
        /// <summary>
        /// Generates a self signed certificate.
        /// </summary>
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
                
                if (IPCheck.Match(x.Trim()).Success)
                {
                    sanBuilder.AddIpAddress(IPAddress.Parse(x.Trim()));
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
                    Settings.AddSettingComments("Settings", "ListenIPAddress", 
                        "Any will default to all the available IP addresses that are assigned to the server. Specify IP if needed.");
                    Settings.AddSetting("Settings", "ListenPort", "12455");
                    Settings.AddSettingComments("Settings", "ListenPort", "The port the TCP server will listen on.");
                    Settings.AddSetting("Settings", "TokenValidTime", "24");
                    Settings.AddSettingComments("Settings", "TokenValidTime", "The hours until your token is valid after authentication using the client.");
                    Settings.AddSetting("Settings", "CertificatePassword", "ChangeMeAndDeleteCertificates");
                    Settings.AddSettingComments("Settings", "CertificatePassword", "Change the default password and delete your pfx file after. Use a generated password.");
                    Settings.AddSetting("Settings", "IpsAndDomains", "127.0.0.1,exampledomains.com");
                    Settings.AddSettingComments("Settings", "IpsAndDomains", 
                        "The IPs and domains the certificate will be signed with. Ensure you give atleast one valid input.",
                        "If you do not connect using one of the values through the client the certificate verification will fail.",
                        "Usually this is the same as the ListenIPAddress value, or if ANY is specified, just specify every assigned domains and ips of your server.");
                    Settings.AddSetting("Settings", "RegistrationSalt", TokenHandler.GetUniqueKey(8));
                    Settings.AddSettingComments("Settings", "RegistrationSalt", "Randomly generated salt for the password hash generation. No need to change this.");
                    Settings.Save();
                }
                Settings = new IniParser(_currentpath + "\\Settings.ini");
                ListenIPAddress = Settings.GetSetting("Settings", "ListenIPAddress");
                ListenPort = int.Parse(Settings.GetSetting("Settings", "ListenPort"));
                TokenValidTime = int.Parse(Settings.GetSetting("Settings", "TokenValidTime"));
                CertificatePassword = Settings.GetSetting("Settings", "CertificatePassword");
                IpsAndDomains = Settings.GetSetting("Settings", "IpsAndDomains");
                RegistrationSalt = Settings.GetSetting("Settings", "RegistrationSalt");
                
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
            User currentuser = null;
            try
            {
                s.ReceiveTimeout = 0;
                s.SendTimeout = 0;
                s.NoDelay = true;
                string ipr = s.RemoteEndPoint.ToString().Split(':')[0];

                NetworkStream stream = new NetworkStream(s);
                SslStream ssl = new SslStream(stream, false);

                ssl.AuthenticateAsServer(Certificate, false, SslProtocols.Tls12 | SslProtocols.Tls13, true);
                while (s.Connected)
                {
                    if (!ssl.CanRead && !stream.DataAvailable)
                    {
                        Thread.Sleep(100);
                        continue;
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

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(leng);
                    }

                    int upcominglength = (BitConverter.ToInt32(leng, 0));
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

                    b = LZ4Compresser.Decompress(b);

                    string message = Encoding.UTF8.GetString(b, 0, b.Length);
                    if (string.IsNullOrEmpty(message) || !message.Contains(Constants.MainSeparator))
                    {
                        ssl.Flush();
                        s.Close();
                        return;
                    }

                    string[] split = message.Split(Constants.MainSeparator);
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

                    string[] otherdata = split[1].Split(Constants.AssistantSeparator);
                    string bmsg = "";
                    switch (code)
                    {
                        case Codes.Login:
                        {
                            try
                            {
                                if (otherdata.Length != 3)
                                {
                                    if (s.Connected)
                                    {
                                        s.Close();
                                    }

                                    return;
                                }

                                string name = otherdata[0].Substring(0, Math.Min(otherdata[0].Length, 50));
                                string password = otherdata[1].Substring(0, Math.Min(otherdata[1].Length, 50));
                                string version = otherdata[2].Substring(0, Math.Min(otherdata[2].Length, 10));

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

                                bmsg = MessageConnector.FormMessage(Codes.Login, "InvalidNameOrPassword");

                                currentuser = PermissionLoader.GetUser(name);
                                if (currentuser != null && !currentuser.IsLoggedIn &&
                                    currentuser.PasswordCheck(password) && version == Program.Version)
                                {
                                    currentuser.Token = TokenHandler.AddNewToken(currentuser.UserName);
                                    currentuser.IsLoggedIn = true;
                                    // TODO: Only send authorized servers.
                                    bmsg = (int) Codes.Login + Constants.MainSeparator + "Success" + Constants.AssistantSeparator + currentuser.Token + Constants.AssistantSeparator +
                                           string.Join(Constants.AssistantSeparator, SquadServerLoader.AllServers.Keys);
                                    Logger.Log("[TCPServer] Authentication from " + ipr + " Name: " + name);
                                }
                                else
                                {
                                    Logger.Log("[TCPServer] Authentication failure from " + ipr + " Name: " + name);
                                }

                                if (s.Connected)
                                {
                                    byte[] messagebyte = asen.GetBytes(bmsg);
                                    byte[] intBytes = BitConverter.GetBytes(messagebyte.Length);
                                    if (BitConverter.IsLittleEndian)
                                        Array.Reverse(intBytes);
                                    ssl.Write(intBytes);
                                    ssl.Write(messagebyte);
                                    if (bmsg.Contains("InvalidNameOrPassword"))
                                    {
                                        s.Close();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError("[Authentication] Error: " + ex);
                            }

                            break;
                        }
                        case Codes.RequestPlayers:
                        {
                            if (otherdata.Length != 2)
                            {
                                if (s.Connected)
                                {
                                    s.Close();
                                }

                                return;
                            }

                            string token = otherdata[0].Substring(0, Math.Min(otherdata[0].Length, 50));
                            string servername = otherdata[1].Substring(0, Math.Min(otherdata[1].Length, 50));

                            if (string.IsNullOrEmpty(token)
                                || string.IsNullOrEmpty(servername))
                            {
                                if (s.Connected)
                                {
                                    s.Close();
                                }

                                return;
                            }

                            if (currentuser != null)
                            {
                                // If token is no longer valid, or doesn't equal with the current one something is wrong.
                                if (currentuser.Token != token || !TokenHandler.HasValidToken(currentuser.UserName) || !currentuser.IsLoggedIn)
                                {
                                    if (s.Connected)
                                    {
                                        s.Close();
                                    }
                                    return;
                                }
                                
                                bmsg = MessageConnector.FormMessage(Codes.RequestPlayers, "Unknown");
                                if (ValidServers.ContainsKey(servername))
                                {
                                    string response = ValidServers[servername].GetPlayerList();
                                    try
                                    {
                                        PlayerListProcesser x = new PlayerListProcesser(response);
                                        string players = JsonParser.Serialize(x.Players);
                                        string disconnectedplayers = JsonParser.Serialize(x.DisconnectedPlayers);
                                        bmsg = MessageConnector.FormMessage(Codes.RequestPlayers, players,
                                            disconnectedplayers);
                                    }
                                    catch (InvalidSquadPlayerListException ex)
                                    {
                                        Logger.LogError("[PlayerListRequest Error] " + ex.Message);
                                    }
                                }
                                
                                if (s.Connected)
                                {
                                    byte[] messagebyte = asen.GetBytes(bmsg);
                                    byte[] intBytes = BitConverter.GetBytes(messagebyte.Length);
                                    if (BitConverter.IsLittleEndian)
                                        Array.Reverse(intBytes);
                                    ssl.Write(intBytes);
                                    ssl.Write(messagebyte);
                                }
                            }
                            else
                            {
                                if (s.Connected)
                                {
                                    s.Close();
                                }

                                return;
                            }

                            break;
                        }
                        case Codes.Disconnect:
                        {
                            if (otherdata.Length != 1)
                            {
                                if (s.Connected)
                                {
                                    s.Close();
                                }

                                return;
                            }

                            string token = otherdata[0].Substring(0, Math.Min(otherdata[0].Length, 50));

                            if (string.IsNullOrEmpty(token))
                            {
                                if (s.Connected)
                                {
                                    s.Close();
                                }

                                return;
                            }

                            if (currentuser != null)
                            {
                                // If token is no longer valid, or doesn't equal with the current one something is wrong.
                                if (currentuser.Token != token || !TokenHandler.HasValidToken(currentuser.UserName) || !currentuser.IsLoggedIn)
                                {
                                    if (s.Connected)
                                    {
                                        s.Close();
                                    }
                                    return;
                                }
                                
                                if (currentuser.Token != null)
                                {
                                    TokenHandler.RemoveToken(currentuser.Token);
                                }

                                currentuser.IsLoggedIn = false;
                                currentuser.Token = null;
                                
                                bmsg = (int) Codes.RequestPlayers + Constants.MainSeparator + "Ok";

                                if (s.Connected)
                                {
                                    byte[] messagebyte = asen.GetBytes(bmsg);
                                    byte[] intBytes = BitConverter.GetBytes(messagebyte.Length);
                                    if (BitConverter.IsLittleEndian)
                                        Array.Reverse(intBytes);
                                    ssl.Write(intBytes);
                                    ssl.Write(messagebyte);
                                    s.Close();
                                }
                            }
                            else
                            {
                                if (s.Connected)
                                {
                                    s.Close();
                                }

                                return;
                            }
                            
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    return;
                }
                
                Logger.LogError("[HandleConnection] General Error: " + ex);
            }
            finally
            {
                if (currentuser != null)
                {
                    if (currentuser.Token != null)
                    {
                        TokenHandler.RemoveToken(currentuser.Token);
                    }

                    currentuser.IsLoggedIn = false;
                    currentuser.Token = null;
                }
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