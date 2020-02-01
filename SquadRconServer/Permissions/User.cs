using System.Collections.Generic;
using SquadRconServer.Tokens;

namespace SquadRconServer.Permissions
{
    internal class User
    {
        private string _username;
        private string _passwordhash;
        private string _token;
        private List<Permissions> _permissions;
        private bool _isloggedin;
        
        public User(string username, string passwordhash, List<Permissions> permissions)
        {
            _username = username;
            _passwordhash = passwordhash;
            _permissions = permissions;
        }

        public string UserName
        {
            get { return _username; }
        }
        
        public string PasswordHash
        {
            get { return _passwordhash; }
        }

        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public bool IsLoggedIn
        {
            get { return _isloggedin; }
            set { _isloggedin = value; }
        }

        public bool PasswordCheck(string password)
        {
            return Crypto.SHA256Hash(password + Server.RegistrationSalt) == _passwordhash;
        }

        public bool HasPermission(Permissions permission)
        {
            return _permissions.Contains(permission) || _permissions.Contains(Permissions.All);
        }

        internal void UpdatePermissions(List<Permissions> permissions)
        {
            _permissions = permissions;
        }
    }
}