using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SquadRconLibrary.JsonSerializable;
using SquadRconServer.Exceptions;

namespace SquadRconServer.ResponseProcessers
{
    public class PlayerListProcesser
    {
        private readonly List<SquadPlayer> _players = new List<SquadPlayer>();
        
        /// <summary>
        /// A messy technique of substring is used to avoid | characters in the player's name.
        /// ----- Active Players -----
        ///  ID: 0 | SteamID: 765611980xxxxxxxx | Name: [EQ]DreTaX | Team ID: 2 | Squad ID: N/A
        /// </summary>
        /// <param name="input"></param>
        public PlayerListProcesser(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new InvalidSquadPlayerListException("Input string was null.");
            }
            
            string[] spl = input.Split("\n");
            foreach (var x in spl)
            {
                if (string.IsNullOrEmpty(x) || x == "----- Active Players -----" || x == "----- Recently Disconnected Players [Max of 15] ----" || !x.Contains("|"))
                {
                    continue;
                }

                string originalstring = x;

                string id = "";
                string steamid = "";
                string name = "";
                string teamid = "";
                string squadid = "";
                
                // Grab ID
                int indexof = originalstring.IndexOf(" | ", 0, StringComparison.CurrentCulture);
                string assist = originalstring.Substring(0, indexof);
                originalstring = originalstring.Replace(assist + " | ", "");

                // Assign ID
                id = assist.Replace("ID: ", "").Trim();

                // Grab SteamID
                indexof = originalstring.IndexOf(" | ", 0, StringComparison.CurrentCulture);
                assist = originalstring.Substring(0, indexof);
                originalstring = originalstring.Replace(assist + " | ", "");
                
                // Assign SteamID
                steamid = assist.Replace("SteamID: ", "").Trim();

                // Leave the player's name for now as it can contains | characters also.
                indexof = originalstring.LastIndexOf(" | ", StringComparison.CurrentCulture);
                assist = originalstring.Substring(indexof, originalstring.Length - indexof);
                originalstring = originalstring.Replace(assist, "");
                // Grab SquadID
                squadid = assist.Replace(" | Squad ID: ", "").Trim();
                
                indexof = originalstring.LastIndexOf(" | ", StringComparison.CurrentCulture);
                assist = originalstring.Substring(indexof, originalstring.Length - indexof);
                originalstring = originalstring.Replace(assist, "");
                
                // Grab TeamID
                teamid = assist.Replace(" | Team ID: ", "").Trim();

                var regex = new Regex(Regex.Escape("Name: "));
                name = regex.Replace(originalstring, "", 1);

                try
                {
                    _players.Add(new SquadPlayer(id, steamid, name, teamid, squadid));
                }
                catch (InvalidSquadPlayerException ex)
                {
                    Logger.LogError("[PlayerListProcesser Error] " + ex.Message);
                }
            }
        }

        public List<SquadPlayer> Players
        {
            get { return _players; }
        }
    }
}