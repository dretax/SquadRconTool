using System.Net;
using SquadRconServer.Exceptions;

namespace SquadRconServer.ServerContainer
{
    public class SquadServer
    {
        private readonly DomainIPContainer _domainIpContainer;
        private readonly int _rconPort;
        private readonly int _queryPort;
        private readonly string _rconPassword;
        private readonly string _serverNickName;
        
        public SquadServer(string serverNickName, string ip, string rconPort, string queryPort, string rconPassword)
        {
            _serverNickName = serverNickName;
            
            IPAddress ipAddress;

            if (Server.IPCheck.Match(ip.Trim()).Success && IPAddress.TryParse(ip, out ipAddress))
            {
                _domainIpContainer = new DomainIPContainer(ipAddress);
            }
            else if (Server.DomainCheck.Match(ip.Trim()).Success)
            {
                _domainIpContainer = new DomainIPContainer(ip.Trim());
            }
            else
            {
                throw new InvalidSquadServerException("Invalid IP or Domain target given in configuration. " + ip);
            }

            int RconPort;
            int QueryPort;

            if (!int.TryParse(rconPort, out RconPort))
            {
                throw new InvalidSquadServerException("Invalid RconPort was given. " + rconPort);
            }
            
            if (!int.TryParse(queryPort, out QueryPort))
            {
                throw new InvalidSquadServerException("Invalid RconPort was given. " + queryPort);
            }

            _queryPort = QueryPort;
            _rconPort = RconPort;
            _rconPassword = rconPassword;
        }

        public DomainIPContainer DomainIPContainer
        {
            get { return _domainIpContainer; }
        }
        
        public int RconPort
        {
            get { return _rconPort; }
        }
        
        public int QueryPort
        {
            get { return _queryPort; }
        }
        
        public string RconPassword
        {
            get { return _rconPassword; }
        }
        
        public string ServerNickName
        {
            get { return _serverNickName; }
        }
    }
}