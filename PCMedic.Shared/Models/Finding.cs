namespace PCMedic.Shared.Models {
  public record Finding(string Id, Severity Severity, string Message, string? SuggestedFix);
}
