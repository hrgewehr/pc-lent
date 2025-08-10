using System.Diagnostics;
using System.Threading.Tasks;

namespace PCMedic.Agent.Services {
  public static class RepairActions {
    static Task<int> Run(string exe, string args) {
      var p = new Process { StartInfo = new ProcessStartInfo {
        FileName = exe, Arguments = args, UseShellExecute = false,
        RedirectStandardOutput = true, RedirectStandardError = true }};
      p.Start(); string o = p.StandardOutput.ReadToEnd(); string e = p.StandardError.ReadToEnd();
      System.IO.Directory.CreateDirectory(@"C:\ProgramData\PCMedic\logs");
      System.IO.File.AppendAllText(@"C:\ProgramData\PCMedic\logs\repair.log", o + e);
      p.WaitForExit(); return Task.FromResult(p.ExitCode);
    }
    public static Task<int> Sfc() => Run("sfc.exe", "/scannow");
    public static Task<int> Dism() => Run("dism.exe", "/Online /Cleanup-Image /RestoreHealth");
    public static Task<int> ScheduleChkdsk(string drive="C:") => Run("cmd.exe", $"/c echo Y|chkdsk {drive} /F");
    public static Task<int> Defrag(string drive="C:") => Run("defrag.exe", $"{drive} /O");
  }
}
