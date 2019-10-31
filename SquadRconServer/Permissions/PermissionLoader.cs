using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                var data = Enum.GetNames(typeof(Permissions)).ToList();
                data.Remove("Unknown");
                File.Create(_currentpath + "\\Permissions.ini").Dispose();
                PermissionsIni = new IniParser(_currentpath + "\\Permissions.ini");
                PermissionsIni.AddSetting("DreTaX", "PasswordHash", "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08");
                PermissionsIni.AddSetting("DreTaX", "Permissions", string.Join(",", Enum.GetNames(typeof(Permissions))));
                PermissionsIni.Save();
            }
            PermissionsIni = new IniParser(_currentpath + "\\Permissions.ini");

            foreach (string x in PermissionsIni.Sections)
            {
                try
                {
                    string pwhash = PermissionsIni.GetSetting(x, "PasswordHash");
                    string permissions = PermissionsIni.GetSetting(x, "Permissions");
                    List<Permissions> perms = new List<Permissions>();
                    foreach (string y in permissions.Split(','))
                    {
                        object obj = null;
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
                    User user = new User(x, pwhash, perms);
                    AllUsers[x] = user;
                }
                catch (Exception ex)
                {
                    Logger.LogError("[Permissions] Failed to read permissions for: " + x + " Fix It, and Reload the Permissions list! " + ex);
                }
            }
        }
    }
}