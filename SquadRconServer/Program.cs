using System;
using System.Net.Sockets;
using SquadRconServer.Permissions;
using SquadRconServer.Tokens;

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
                        Console.WriteLine("Please enter a username");
                        string username = Console.ReadLine();
                        Console.WriteLine("Please enter a password. I suggest generating one.");
                        string password = Console.ReadLine();
                        Console.WriteLine("Username: " + username);
                        Console.WriteLine("Password: " + password);
                        Console.WriteLine("Add user? (Y / N) Default permissions will be applied.");
                        string ok = Console.ReadLine();
                        if (ok != null && ok.ToLower() == "y")
                        {
                            PermissionLoader.AddUser(username, password);
                            Console.WriteLine(username + " added. Edit permissions in the ini file, and reload.");
                        }
                        break;
                    case "changepassword":
                        break;
                    case "removeuser":
                        break;
                    case "cleartokens":
                        TokenHandler.ClearTokens();
                        Logger.Log("Tokens cleared!");
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

        private static void DisplayHelp()
        {
            
        }
        
    }
}