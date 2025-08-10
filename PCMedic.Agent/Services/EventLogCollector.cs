using System;
using System.Diagnostics.Eventing.Reader;

namespace PCMedic.Agent.Services {
  public static class EventLogCollector {
    public static (int word, int excel) CountOfficeCrashesLast24h() {
      int w = 0, e = 0;
      string q = "*[System[Level=2 and TimeCreated[timediff(@SystemTime) <= 86400000]]] and *[EventData[Data='WINWORD.EXE' or Data='EXCEL.EXE']]";
      var query = new EventLogQuery("Application", PathType.LogName, q);
      using var rdr = new EventLogReader(query);
      for (EventRecord rec = rdr.ReadEvent(); rec != null; rec = rdr.ReadEvent()) {
        string txt = rec.FormatDescription() ?? "";
        if (txt.Contains("WINWORD.EXE", StringComparison.OrdinalIgnoreCase)) w++;
        if (txt.Contains("EXCEL.EXE", StringComparison.OrdinalIgnoreCase)) e++;
      }
      return (w, e);
    }
  }
}
