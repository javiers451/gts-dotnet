using Gts.Extraction;
using Gts.Store.InMemory;

namespace Gts.Store;

public abstract class GtsRegistry
{
    private readonly IGtsStore _store;
    
    public GtsRegistryConfig Config { get; }

    protected GtsRegistry(IGtsStore store, GtsRegistryConfig config)
    {
        _store = store;
        Config = config;
    }
    
    public ValueTask SaveAsync(GtsJsonEntity entity)
    {
        // TODO: validation logic
        return _store.SaveAsync(entity);
    }

    public ValueTask<GtsJsonEntity?> GetAsync(GtsId id)
    {
        return _store.GetAsync(id);
    }

    public ValueTask<IEnumerable<GtsJsonEntity>> GetAllAsync()
    {
        return _store.GetAllAsync();
    }

    public ValueTask<int> CountAsync()
    {
        return _store.CountAsync();
    }

    public static GtsRegistry InMemory(GtsRegistryConfig config)
    {
        return InMemoryGtsRegistry.Simple(config);
    }

    public static GtsRegistry InMemoryThreadSafe(GtsRegistryConfig config)
    {
        return InMemoryGtsRegistry.Concurrent(config);
    }
}
