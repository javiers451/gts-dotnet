using System.Collections.Concurrent;
using Gts.Extraction;

namespace Gts.Store.InMemory;

internal class InMemoryGtsRegistry : GtsRegistry
{
    private InMemoryGtsRegistry(IGtsStore store, GtsRegistryConfig config)
        : base(store, config)
    {
    }

    internal static InMemoryGtsRegistry Simple(GtsRegistryConfig config)
    {
        return new InMemoryGtsRegistry(
            new InMemoryGtsStore<Dictionary<GtsId, GtsJsonEntity>>(), config);
    }

    internal static InMemoryGtsRegistry Concurrent(GtsRegistryConfig config)
    {
        return new InMemoryGtsRegistry(
            new InMemoryGtsStore<ConcurrentDictionary<GtsId, GtsJsonEntity>>(), config);
    }
}
