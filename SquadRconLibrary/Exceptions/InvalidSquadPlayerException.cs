using System;

namespace SquadRconServer.Exceptions
{
    public class InvalidSquadPlayerException : Exception
    {
        public InvalidSquadPlayerException(string error)
            : base(error)
        {

        }
    }
}