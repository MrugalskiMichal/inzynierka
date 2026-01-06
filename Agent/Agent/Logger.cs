using Mono.Unix.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Agent
{
    public static class Logger
    {
        private static readonly string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agent.log");
        private static readonly object _lockFile = new object();

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Error(string message)
        {
            Write("ERROR", message);
        }

        public static void Warning(string message)
        {
            Write("WARN", message);
        }

        private static void Write(string level, string message)
        {
            lock (_lockFile)
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
                File.AppendAllText(filePath, logEntry + Environment.NewLine);
            }
        }
    }
}
