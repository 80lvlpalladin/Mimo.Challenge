namespace Mimo.Challenge.Domain.Abstractions;

public interface IAcademicUnitRepository
{
    public ValueTask<string?> GetPathForUnitAsync(uint unitId, CancellationToken ct = default);
    public ValueTask<uint[]> GetChildrenIdsAsync(uint? unitId = null, CancellationToken ct = default);
    public ValueTask<bool> CheckIfUnitExistsAsync(uint unitId, CancellationToken ct = default);
}