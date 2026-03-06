using Gts.Extraction;

namespace Gts.Store;

/// <summary>Storage abstraction for GTS JSON entities keyed by GTS ID.</summary>
public interface IGtsStore
{
    /// <summary>Stores or overwrites the entity (keyed by its GTS ID).</summary>
    ValueTask SaveAsync(GtsJsonEntity entity);
    
    /// <summary>Retrieves an entity by GTS ID, or null if not found.</summary>
    ValueTask<GtsJsonEntity?> GetAsync(GtsId id);
    
    /// <summary>Returns all stored entities.</summary>
    ValueTask<IList<GtsJsonEntity>> GetAllAsync();
    
    /// <summary>Returns the number of stored entities.</summary>
    ValueTask<int> CountAsync();
    
    //ValueTask<int> ValidateAsync(); // TODO:
}
