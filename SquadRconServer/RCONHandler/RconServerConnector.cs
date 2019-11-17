using System.Net;
using QueryMaster;
using QueryMaster.GameServer;

namespace SquadRconServer.RCONHandler
{
    public class RconServerConnector
    {
        private QueryMaster.GameServer.Server squadServer = null;
        private ServerConnectionInfo serverConnectionInfo;
        
        public bool Connect(ServerConnectionInfo connectionInfo)
        {
            this.serverConnectionInfo = connectionInfo;
            this.squadServer = ServerQuery.GetServerInstance(EngineType.Source, new IPEndPoint(this.serverConnectionInfo.ServerIP, this.serverConnectionInfo.ServerRconPort));
            return this.squadServer.GetControl(this.serverConnectionInfo.RCONPassword);
        }

        public void Disconnect()
        {
            if (squadServer != null)
            {
                this.squadServer.Dispose();
            }
        }

        public string GetPlayerList()
        {
            if (squadServer != null && this.squadServer.GetControl(this.serverConnectionInfo.RCONPassword))
            {
                return this.squadServer.Rcon.SendCommand("ListPlayers", true);
            }
            return "Unexpected error! Unable to communicate with the Squad Server!";
        }

        public string SendCommand(string command)
        {
            if (squadServer != null && this.squadServer.GetControl(this.serverConnectionInfo.RCONPassword))
            {
                return this.squadServer.Rcon.SendCommand(command, true);
            }
            return "Unexpected error! Unable to communicate with the Squad Server!";
        }

        public ServerInfo GetServerData()
        {
            using (QueryMaster.GameServer.Server s = ServerQuery.GetServerInstance(EngineType.Source, new IPEndPoint(this.serverConnectionInfo.ServerIP, this.serverConnectionInfo.ServerQueryPort)))
            {
                return s.GetInfo();
            }
        }

    }
}