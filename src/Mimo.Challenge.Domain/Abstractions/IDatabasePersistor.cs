namespace Mimo.Challenge.Domain.Abstractions;

public interface IDatabasePersistor
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}