using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SquadRconServer.Tokens;

namespace SquadRconServer.Permissions
{
    internal enum Permissions
    {
        Kick,
        Ban,
        Unknown
    }
    
    internal static class PermissionLoader
    {
        private static string _currentpath = Directory.GetCurrentDirectory();
        internal static readonly Dictionary<string, User> AllUsers = new Dictionary<string, User>();
        internal static IniParser PermissionsIni;

        internal static void LoadPermissions()
        {
            AllUsers.Clear();
            if (!File.Exists(_currentpath + "\\Permissions.ini"))
            {
                File.Create(_currentpath + "\\Permissions.ini").Dispose();
                PermissionsIni = new IniParser(_currentpath + "\\Permissions.ini");
                PermissionsIni.AddSetting("dretax", "PasswordHash", "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08");
                PermissionsIni.AddSetting("dretax", "Permissions", string.Join(",", GetDefaultPermissions()));
                PermissionsIni.Save();
            }
            PermissionsIni = new IniParser(_currentpath + "\\Permissions.ini");

            foreach (string x in PermissionsIni.Sections)
            {
                try
                {
                    string pwhash = PermissionsIni.GetSetting(x, "PasswordHash");
                    string permissions = PermissionsIni.GetSetting(x, "Permissions");
                    
                    User user = new User(x, pwhash, ProcessPermissions(permissions));
                    AllUsers[x] = user;
                }
                catch (Exception ex)
                {
                    Logger.LogError("[Permissions] Failed to read permissions for: " + x + " Fix It, and Reload the Permissions list! " + ex);
                }
            }
        }

        internal static bool AddUser(string username, string password)
        {
            if (PermissionsIni.GetSetting(username.ToLower(), "PasswordHash") == null)
            {
                return false;
            }

            string pwhash = Crypto.SHA256Hash(password + Server.RegistrationSalt);
            string perms = string.Join(",", GetDefaultPermissions());
            PermissionsIni.AddSetting(username.ToLower(), "PasswordHash", Crypto.SHA256Hash(password + Server.RegistrationSalt));
            PermissionsIni.AddSetting(username.ToLower(), "Permissions", perms);
            PermissionsIni.Save();
            
            User user = new User(username.ToLower(), pwhash, ProcessPermissions(perms));
            AllUsers[username.ToLower()] = user;
            
            return true;
        }

        private static List<Permissions> ProcessPermissions(string permissions)
        {
            List<Permissions> perms = new List<Permissions>();
            foreach (string y in permissions.Split(','))
            {
                if (string.IsNullOrEmpty(y)) continue;
                        
                object obj;
                Enum.TryParse(typeof(Permissions), y.Trim(), true, out obj);
                if (obj != null)
                {
                    Permissions perm = obj is Permissions ? (Permissions) obj : Permissions.Unknown;
                    if (!perms.Contains(perm))
                    {
                        perms.Add(perm);
                    }
                }
            }

            return perms;
        }

        private static List<string> GetDefaultPermissions()
        {
            var data = Enum.GetNames(typeof(Permissions)).ToList();
            data.Remove("Unknown");
            return data;
        }
    }
}