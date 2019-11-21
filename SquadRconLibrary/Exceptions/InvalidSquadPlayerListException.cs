using System;

namespace SquadRconServer.Exceptions
{
    public class InvalidSquadPlayerListException : Exception
    {
        public InvalidSquadPlayerListException(string error)
            : base(error)
        {

        }
    }
}