using Gts.Extraction;
using Gts.Store.InMemory;

namespace Gts.Store;

/// <summary>Registry for GTS JSON entities with configurable storage and validation.</summary>
public abstract class GtsRegistry
{
    private readonly IGtsStore _store;
    
    /// <summary>Registry configuration (e.g. reference validation).</summary>
    public GtsRegistryConfig Config { get; }

    /// <summary>Initializes the registry with the given store and config.</summary>
    protected GtsRegistry(IGtsStore store, GtsRegistryConfig config)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(config);
        
        _store = store;
        Config = config;
    }
    
    /// <summary>Stores or overwrites the entity in the registry.</summary>
    public ValueTask SaveAsync(GtsJsonEntity entity)
    {
        // TODO: validation logic
        return _store.SaveAsync(entity);
    }

    /// <summary>Retrieves an entity by GTS ID, or null if not found.</summary>
    public ValueTask<GtsJsonEntity?> GetAsync(GtsId id)
    {
        return _store.GetAsync(id);
    }

    /// <summary>Returns all entities in the registry.</summary>
    public ValueTask<IList<GtsJsonEntity>> GetAllAsync()
    {
        return _store.GetAllAsync();
    }

    /// <summary>Returns the number of entities in the registry.</summary>
    public ValueTask<int> CountAsync()
    {
        return _store.CountAsync();
    }

    /// <summary>Creates an in-memory registry (single-threaded).</summary>
    public static GtsRegistry InMemory(GtsRegistryConfig config)
    {
        return InMemoryGtsRegistry.Simple(config);
    }

    /// <summary>Creates an in-memory registry with thread-safe storage.</summary>
    public static GtsRegistry InMemoryThreadSafe(GtsRegistryConfig config)
    {
        return InMemoryGtsRegistry.Concurrent(config);
    }
}
