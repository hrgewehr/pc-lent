using PCMedic.Shared.Models;

namespace PCMedic.Agent.Services;

public class SnapshotStore
{
    private readonly object _lock = new();
    private HealthSnapshot _current = new(DateTimeOffset.MinValue, "Unknown");

    public HealthSnapshot Current
    {
        get { lock (_lock) return _current; }
        set { lock (_lock) _current = value; }
    }
}
