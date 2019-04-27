using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KFMarketAnalysis.Models.Utility
{
    public static class Logging
    {
        private static readonly string logPath = $"temp/logs";

        public static void AddToLog(object source, Exception e)
        {
            CreateTempDirectory();

            var now = DateTime.Now;

            var f = new FileStream($"{logPath}/errorlog_{now.Day}_{now.Month}_{now.Year}.log", FileMode.Append);

            var sw = new StreamWriter(f);

            sw.WriteLine($"[{now.Day}.{now.Month}.{now.Year} " +
                $"{now.Hour}:{now.Minute}:{now.Second}] " +
                $"Object: {source}\n\t\t" +
                $"Message: {e.Message}\n\t\t" +
                $"InnerException: {e.InnerException}");

            sw.Close();
        }

        private static void CreateTempDirectory()
        {
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);
        }
    }
}
