using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace GitRepoTask.Services
{
    public interface ILoggingService
    {
        void LogError(string message, Exception ex = null);
        void LogInfo(string message);
    }
    public class LoggingService : ILoggingService 
    {
        private readonly string _logPath;

        public LoggingService()
        {
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
        }

        public void LogError(string message, Exception ex = null)
        {
            string logMessage = $"[ERROR] {DateTime.Now}: {message}";
            if (ex != null)
            {
                logMessage += $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }
            WriteToFile(logMessage);
        }

        public void LogInfo(string message)
        {
            string logMessage = $"[INFO] {DateTime.Now}: {message}";
            WriteToFile(logMessage);
        }

        private void WriteToFile(string message)
        {
            try
            {
                string filePath = Path.Combine(_logPath, $"log_{DateTime.Now.ToString("yyyyMMdd")}.txt");
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine(message);
                    writer.WriteLine(new string('-', 50));
                }
            }
            catch
            {
                // Failed to write to log file - could add fallback logging here
            }
        }
    }
}