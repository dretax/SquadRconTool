using System.Net;

namespace SquadRconServer.ServerContainer
{
    public class DomainIPContainer
    {
        private readonly IPAddress _ip;

        public DomainIPContainer(string domain)
        {
            var address = Dns.GetHostAddresses(domain);
            if (address.Length > 0)
            {
                _ip = address[0];
            }
        }

        public DomainIPContainer(IPAddress ip)
        {
            _ip = ip;
        }

        public IPAddress IP
        {
            get { return _ip; }
        }
    }
}