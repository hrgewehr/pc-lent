using PCMedic.Shared.Models;
namespace PCMedic.Agent.Services {
  public class SnapshotStore {
    private readonly object _lock = new();
    private HealthSnapshot _current = new(System.DateTimeOffset.MinValue, "Unknown",
      new(), new(0,0,0,0,0), 0,0, 0, 100,100, new());
    public HealthSnapshot Current { get { lock (_lock) return _current; } set { lock (_lock) _current = value; } }
  }
}
