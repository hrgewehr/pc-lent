namespace PCMedic.Shared.Models {
  public record DiskSmart(string DeviceId, string Model, bool? PredictFailure, int? ReallocatedSectors, string MediaType);
}
