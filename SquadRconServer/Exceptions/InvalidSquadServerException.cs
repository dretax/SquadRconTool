using System;

namespace SquadRconServer.Exceptions
{
    public class InvalidSquadServerException : Exception
    {
        public InvalidSquadServerException(string error)
            : base(error)
        {

        }
    }
}