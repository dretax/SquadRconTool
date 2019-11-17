using System.Net;

namespace SquadRconServer.RCONHandler
{
    public class ServerConnectionInfo
    {
        public ServerConnectionInfo(IPAddress serverIp, int serverRconPort, int serverQueryPort, string rconPassword)
        {
            ServerIP = serverIp;
            ServerRconPort = serverRconPort;
            RCONPassword = rconPassword;
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
    }
}