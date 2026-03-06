using Gts.Extraction;

namespace Gts.Store.InMemory;

/// <summary>In-memory implementation of <see cref="IGtsStore"/> using a dictionary-like backing store.</summary>
internal class InMemoryGtsStore<T> : IGtsStore
    where T : class, IDictionary<GtsId, GtsJsonEntity>, new()
{
    private readonly T _entities = new();

    /// <inheritdoc/>
    public ValueTask SaveAsync(GtsJsonEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var key = entity.GtsId;
        
        ArgumentNullException.ThrowIfNull(key);
        
        _entities[key] = entity;
        
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask<GtsJsonEntity?> GetAsync(GtsId id)
    {
        _entities.TryGetValue(id, out var entity);
        return ValueTask.FromResult(entity);
    }

    /// <inheritdoc/>
    public ValueTask<IList<GtsJsonEntity>> GetAllAsync()
    {
        return ValueTask.FromResult<IList<GtsJsonEntity>>(_entities.Values.ToArray());
    }

    /// <inheritdoc/>
    public ValueTask<int> CountAsync()
    {
        return ValueTask.FromResult(_entities.Count);
    }
}
