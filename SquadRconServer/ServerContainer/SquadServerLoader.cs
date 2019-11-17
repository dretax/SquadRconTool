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
                SquadServersIni = new IniParser(_currentpath + "\\SquadServers.ini");
                SquadServersIni.AddSetting("ServerName", "IpOrDomain", "127.0.0.1");
                SquadServersIni.AddSettingComments("ServerName", "IpOrDomain", "Your Squad Server's IP, or Domain address goes here.");
                SquadServersIni.AddSetting("ServerName", "QueryPort", "27165");
                SquadServersIni.AddSettingComments("ServerName", "QueryPort", "The query port of this server.");
                SquadServersIni.AddSetting("ServerName", "RconPort", "21114");
                SquadServersIni.AddSettingComments("ServerName", "RconPort", "The rcon port of this server.");
                SquadServersIni.AddSetting("ServerName", "RconPassword", "test");
                SquadServersIni.AddSettingComments("ServerName", "RconPassword", "The rcon password of this server.");
                SquadServersIni.Save();
            }
            SquadServersIni = new IniParser(_currentpath + "\\SquadServers.ini");

            foreach (IniParser.IniSection x in SquadServersIni.Sections.Values)
            {
                try
                {
                    string IpOrDomain = SquadServersIni.GetSetting(x.SectionName, "IpOrDomain");
                    string QueryPort = SquadServersIni.GetSetting(x.SectionName, "QueryPort");
                    string RconPort = SquadServersIni.GetSetting(x.SectionName, "RconPort");
                    string RconPassword = SquadServersIni.GetSetting(x.SectionName, "RconPassword");
                    
                    SquadServer server = new SquadServer(x.SectionName, IpOrDomain, RconPort, QueryPort, RconPassword);
                    AllServers[x.SectionName] = server;
                }
                catch (InvalidSquadServerException ex)
                {
                    Logger.LogError("[SquadServers] Failed to read values for: " + x + " Fix It! " + ex);
                }
            }
        }
    }
}