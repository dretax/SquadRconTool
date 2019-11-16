using System;

namespace SquadRconServer.Tokens
{
    internal class TokenHolder
    {
        private string _key;
        private DateTime _date;
        
        public TokenHolder(string key, DateTime now)
        {
            _key = key;
            _date = now;
        }

        public DateTime TokenDate
        {
            get { return _date; }
        }

        public string Token
        {
            get { return _key; }
        }
    }
}