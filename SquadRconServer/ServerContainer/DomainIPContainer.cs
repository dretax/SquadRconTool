using System.Net;

namespace SquadRconServer.ServerContainer
{
    public class DomainIPContainer
    {
        private readonly IPAddress _ip;
        private readonly string _domain;
        private readonly bool _isdomain;

        public DomainIPContainer(string domain)
        {
            _domain = domain;
            _isdomain = true;
        }

        public DomainIPContainer(IPAddress ip)
        {
            _ip = ip;
            _isdomain = false;
        }

        public string Domain
        {
            get { return _domain; }
        }

        public IPAddress IP
        {
            get { return _ip; }
        }

        public bool IsDomain
        {
            get { return _isdomain; }
        }
    }
}