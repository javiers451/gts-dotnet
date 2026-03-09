using System.Text.Json;
using System.Text.Json.Nodes;

namespace Gts.Extraction;

/// <summary>
/// ID Extraction: extract GTS identifiers from JSON objects using System.Text.Json.
/// Delegates to <see cref="GtsJsonEntity"/>.
/// </summary>
public static class GtsExtract
{
    /// <summary>
    /// Extracts the primary entity/schema ID from a JSON object.
    /// </summary>
    public static ExtractResult ExtractId(JsonObject json, GtsExtractOptions? options = null)
        => GtsJsonEntity.ExtractId(json, options);

    /// <summary>
    /// Extracts ID if the root is a JSON object; otherwise returns a result with null Id.
    /// </summary>
    public static ExtractResult ExtractId(JsonNode? node, GtsExtractOptions? options = null)
        => GtsJsonEntity.ExtractId(node, options);

    /// <summary>
    /// Extracts ID from a JSON object element (e.g. from JsonDocument.RootElement).
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when element is not ValueKind.Object.</exception>
    public static ExtractResult ExtractId(JsonElement element, GtsExtractOptions? options = null)
        => GtsJsonEntity.ExtractId(element, options);

    /// <summary>
    /// Builds a GtsJsonEntity with primary ID, schema ID, and all GTS references in the tree.
    /// </summary>
    public static GtsJsonEntity ExtractEntity(JsonObject json, GtsExtractOptions? options = null)
        => GtsJsonEntity.ExtractEntity(json, options);

    /// <summary>
    /// Walks the JSON tree and returns every string that is a valid GTS ID, with its path.
    /// </summary>
    public static IReadOnlyList<GtsReference> ExtractReferences(JsonNode? node)
        => GtsJsonEntity.ExtractReferences(node);
}
