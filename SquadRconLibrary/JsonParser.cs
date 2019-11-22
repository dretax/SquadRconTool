using Newtonsoft.Json;

namespace SquadRconLibrary
{
    public static class JsonParser
    {
        /// <summary>
        /// When serialization or deserialization fails I need the function
        /// to return null instead of catching the exception.
        /// It's easier to handle incorrect data this way.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch
            {
                // ignore
            }

            return null;
        }

        /// <summary>
        /// When serialization or deserialization fails I need the function
        /// to return null instead of catching the exception.
        /// It's easier to handle incorrect data this way.
        /// </summary>
        /// <param name="s"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T DeSerialize<T>(string s)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(s);
            }
            catch
            {
                // ignore
            }

            return default(T);
        }
    }
}