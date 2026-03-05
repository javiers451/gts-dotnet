using Gts.Extraction;

namespace Gts.Store;

public interface IGtsStore
{
    ValueTask SaveAsync(GtsJsonEntity entity);
    
    ValueTask<GtsJsonEntity?> GetAsync(GtsId id);
    
    ValueTask<IEnumerable<GtsJsonEntity>> GetAllAsync();
    
    ValueTask<int> CountAsync();
    
    //ValueTask<int> ValidateAsync(); // TODO:
}
