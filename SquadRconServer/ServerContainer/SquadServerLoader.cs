using System;
using System.Collections.Generic;
using System.IO;
using SquadRconServer.Exceptions;
using SquadRconServer.Permissions;

namespace SquadRconServer.ServerContainer
{
    internal static class SquadServerLoader
    {
        private static string _currentpath = Directory.GetCurrentDirectory();
        internal static readonly Dictionary<string, SquadServer> AllServers = new Dictionary<string, SquadServer>();
        internal static IniParser SquadServersIni;

        internal static void LoadServers()
        {
            AllServers.Clear();
            if (!File.Exists(_currentpath + "\\SquadServers.ini"))
            {
                File.Create(_currentpath + "\\SquadServers.ini").Dispose();
                SquadServersIni = new IniParser(_currentpath + "\\Permissions.ini");
                SquadServersIni.AddSetting("ServerName", "IpOrDomain", "127.0.0.1");
                SquadServersIni.AddSetting("ServerName", "QueryPort", "27165");
                SquadServersIni.AddSetting("ServerName", "RconPort", "21114");
                SquadServersIni.AddSetting("ServerName", "RconPassword", "test");
                SquadServersIni.Save();
            }
            SquadServersIni = new IniParser(_currentpath + "\\SquadServers.ini");

            foreach (string x in SquadServersIni.Sections)
            {
                try
                {
                    string IpOrDomain = SquadServersIni.GetSetting(x, "IpOrDomain");
                    string QueryPort = SquadServersIni.GetSetting(x, "QueryPort");
                    string RconPort = SquadServersIni.GetSetting(x, "RconPort");
                    string RconPassword = SquadServersIni.GetSetting(x, "RconPassword");
                    
                    SquadServer server = new SquadServer(x, IpOrDomain, RconPort, QueryPort, RconPassword);
                    AllServers[x] = server;
                }
                catch (InvalidSquadServerException ex)
                {
                    Logger.LogError("[SquadServers] Failed to read values for: " + x + " Fix It! " + ex);
                }
            }
        }
    }
}