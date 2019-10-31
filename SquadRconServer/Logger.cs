using System;
using System.Diagnostics;
using System.IO;

namespace SquadRconServer
{
    internal static class Logger
    {
        private struct Writer
        {
            public StreamWriter LogWriter;
            public string DateTime;
        }

        private static string LogsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        private static Writer LogWriter;

        internal static void Init()
        {
            try
            {
                Directory.CreateDirectory(LogsFolder);
                LogWriterInit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        

        private static void LogWriterInit()
        {
            try
            {
                if (LogWriter.LogWriter != null)
                    LogWriter.LogWriter.Close();

                LogWriter.DateTime = DateTime.Now.ToString("dd_MM_yyyy");
                LogWriter.LogWriter = new StreamWriter(Path.Combine(LogsFolder, string.Format("Log_{0}.log", LogWriter.DateTime)), true);
                LogWriter.LogWriter.AutoFlush = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static string LogFormat(string Text)
        {
            Text = "[" + DateTime.Now + "] " + Text;
            return Text;
        }

        private static void WriteLog(string Message)
        {
            try
            {
                if (LogWriter.DateTime != DateTime.Now.ToString("dd_MM_yyyy"))
                    LogWriterInit();
                LogWriter.LogWriter.WriteLine(LogFormat(Message));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void Log(string Message)
        {
            Console.WriteLine(Message);
            Message = "[Console] " + Message;
            WriteLog(Message);
        }
        

        public static void LogWarning(string Message)
        {
            Console.WriteLine(Message);
            Message = "[Warning] " + Message;
            WriteLog(Message);
        }

        public static void LogError(string Message)
        {
            Console.WriteLine(Message);
            Message = "[Error] " + Message;
            WriteLog(Message);
        }
        

        public static void LogDebug(string Message)
        {
            Console.WriteLine("[DEBUG] " + Message);
            Message = "[Debug] " + Message;
            WriteLog(Message);
        }
    }
}