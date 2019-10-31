using System.Collections.Generic;

namespace SquadRconServer.Permissions
{
    internal class User
    {
        private string _username;
        private string _passwordhash;
        private List<Permissions> _permissions;
        
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

        public bool HasPermission(Permissions permission)
        {
            return _permissions.Contains(permission);
        }

        internal void UpdatePermissions(List<Permissions> permissions)
        {
            _permissions = permissions;
        }
    }
}