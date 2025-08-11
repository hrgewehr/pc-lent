using System;
using System.IO;

namespace PCMedic.Agent.Services {
  public static class OpLogger {
    private static readonly string LogDir = @"C:\\ProgramData\\PCMedic\\logs";
    private static readonly string OpLog = Path.Combine(LogDir, "operations.log");
    public static void Log(string message) {
      try {
        Directory.CreateDirectory(LogDir);
        File.AppendAllText(OpLog, $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
      } catch { }
    }
  }
}
