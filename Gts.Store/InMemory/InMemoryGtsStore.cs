using Gts.Extraction;

namespace Gts.Store.InMemory;

internal class InMemoryGtsStore<T> : IGtsStore
    where T : class, IDictionary<GtsId, GtsJsonEntity>, new()
{
    private readonly T _entities = new();

    public ValueTask SaveAsync(GtsJsonEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var key = entity.GtsId;
        
        ArgumentNullException.ThrowIfNull(key);
        
        _entities[key] = entity;
        
        return ValueTask.CompletedTask;
    }

    public ValueTask<GtsJsonEntity?> GetAsync(GtsId id)
    {
        _entities.TryGetValue(id, out var entity);
        return ValueTask.FromResult(entity);
    }

    public ValueTask<IEnumerable<GtsJsonEntity>> GetAllAsync()
    {
        return ValueTask.FromResult(_entities.Values.AsEnumerable());
    }

    public ValueTask<int> CountAsync()
    {
        return ValueTask.FromResult(_entities.Count);
    }
}
