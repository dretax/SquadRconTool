using System;
using System.Net.Sockets;
using SquadRconServer.Permissions;

namespace SquadRconServer
{
    public class Program
    {
        public const string Author = "DreTaX";
        public const string Version = "1.0";
        public const string Github = "https://github.com/dretax/SquadRconTool";
        public const string WebSite = "https://equinoxgamers.com";
        public const string Discord = "Discordlink";
        private static Server _srv;
        private static bool _isrunning = true;

        public static void Main(string[] args)
        {
            Logger.Init();
            Logger.Log("Squad Rcon Bridge Server Created by " + Author + " v" + Version);
            Logger.Log("Respository Link: " + Github);
            Logger.Log("More Contact: " + WebSite + " " + Discord);
            Logger.Log("Initializing TCP...");
            _srv = new Server();

            while (_isrunning)
            {
                string input = Console.ReadLine();
                switch (input)
                {
                    case "quit":
                        _srv.StopConnections();
                        _isrunning = false;
                        Logger.Log("Shutting down TCP Server.");
                        break;
                    case "reloadperms":
                        PermissionLoader.LoadPermissions();
                        Logger.Log("Permissions reloaded.");
                        break;
                    case "adduser":
                        break;
                    case "changepassword":
                        break;
                    case "removeuser":
                        break;
                    case "help":
                        break;
                    default:
                        Logger.Log("Unknown command. Type 'help' to display all of the commands.");
                        break;
                }
            }
            Logger.Log("Press something to exit...");
            Console.ReadKey();
        }
        
    }
}