using Mimo.Challenge.Domain.Abstractions;

namespace Mimo.Challenge.DAL;

public class DatabasePersistor(MimoContext context) : IDatabasePersistor
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => 
        context.SaveChangesAsync(cancellationToken);
}