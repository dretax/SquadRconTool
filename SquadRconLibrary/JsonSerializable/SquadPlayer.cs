using SquadRconServer.Exceptions;

namespace SquadRconLibrary.JsonSerializable
{
    public class SquadPlayer
    {
        private int _id;
        private ulong _steamid;
        private string _name;
        private string _teamid;
        private string _squadid;
        
        public SquadPlayer(string id, string steamid, string name, string teamid, string squadid)
        {
            if (!int.TryParse(id, out _id) || !ulong.TryParse(steamid, out _steamid))
            {
                throw new InvalidSquadPlayerException("Failed to parse " + id + " or " + steamid + ".");
            }
            
            _name = name;
            _teamid = teamid;
            _squadid = squadid;
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public ulong SteamID
        {
            get { return _steamid; }
            set { _steamid = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        
        public string TeamID
        {
            get { return _teamid; }
            set { _teamid = value; }
        }
        
        public string SquadID
        {
            get { return _squadid; }
            set { _squadid = value; }
        }
    }
}