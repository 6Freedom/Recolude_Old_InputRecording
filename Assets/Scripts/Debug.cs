using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace SixFreedom
{
    public static class Debug
    {
        private static string logFilePath;
        private static bool logInFile = false;
        private static bool warningInFile = false;
        private static bool errorInFile = true;
        public static void Initialize()
        {
            UnityEngine.CrashReportHandler.CrashReportHandler.SetUserMetadata("deviceUniqueID", SystemInfo.deviceUniqueIdentifier);
            logFilePath = Path.Combine(Application.persistentDataPath, "SixFreedomLog.txt");
            if(!File.Exists(logFilePath))
            {
                Log("Logger file created at {logFilePath}");
                var myFile = File.Create(logFilePath);
                myFile.Close();
            }
        }

        static public void Log(string _message, UnityEngine.Object _context = null)
        {
            string message = $"[6freedom] [Log] {_message}";
            UnityEngine.Debug.Log(message, _context);
            if(logInFile)
                File.AppendAllText(logFilePath, $"\n\n[{DateTime.Now:HH:mm:ss}] {message}\n{Environment.StackTrace}");

        }
        static public void LogWarning(string _message, UnityEngine.Object _context = null)
        {
            string message = $"[6freedom] [Warning] {_message}";
            UnityEngine.Debug.LogWarning(message, _context);
            if (warningInFile)
                File.AppendAllText(logFilePath, $"\n\n[{DateTime.Now:HH:mm:ss}] {message}\n{Environment.StackTrace}");
        }
        static public void LogError(string _message, UnityEngine.Object _context = null)
        {
            string message = $"[6freedom] [Error] {_message}";
            UnityEngine.Debug.LogError(message, _context);
            if (errorInFile)
                File.AppendAllText(logFilePath, $"\n\n[{DateTime.Now:HH:mm:ss}] {message}\n{Environment.StackTrace}");
        }
        static public void LogException(Exception _exception, UnityEngine.Object _context = null)
        {
            string message = $"[6freedom] [Exception] {_exception.Message}";
            UnityEngine.Debug.LogException(_exception, _context);
            if (errorInFile)
                File.AppendAllText(logFilePath, $"\n\n[{DateTime.Now:HH:mm:ss}] {message}\n{Environment.StackTrace}");
        }
    }

}
