using System.Net;

namespace SquadRconServer.RCONHandler
{
    public class ServerConnectionInfo
    {
        public ServerConnectionInfo(IPAddress serverIp, int serverRconPort, int serverQueryPort, string rconPassword, string adminDisplayName = "SquadRconServer Admin")
        {
            ServerIP = serverIp;
            ServerRconPort = serverRconPort;
            RCONPassword = rconPassword;
            AdminDisplayName = adminDisplayName;
            ServerQueryPort = serverQueryPort;
        }

        public IPAddress ServerIP
        {
            get; 
            private set;
        }

        public int ServerRconPort
        {
            get;
            private set;
        }
        
        public int ServerQueryPort
        {
            get;
            private set;
        }

        public string RCONPassword
        {
            get; 
            private set;
        }
        
        public string AdminDisplayName 
        { 
            get;
            private set;
        }

        public bool IsValid()
        {
            return this.ServerIP != null && this.ServerQueryPort != 0 && this.ServerRconPort != 0 && !string.IsNullOrEmpty(RCONPassword);
        }
    }
}