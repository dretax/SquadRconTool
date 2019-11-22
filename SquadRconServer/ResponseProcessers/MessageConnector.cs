using System.Security.AccessControl;
using SquadRconLibrary;

namespace SquadRconServer.ResponseProcessers
{
    public static class MessageConnector
    {
        public static string FormMessage(Codes code, params string[] parameters)
        {
            return (int) code + Constants.MainSeparator + string.Join(Constants.AssistantSeparator, parameters);
        }
    }
}