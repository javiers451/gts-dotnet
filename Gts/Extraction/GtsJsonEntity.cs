using System.Text.Json.Nodes;

namespace Gts.Extraction;

/// <summary>
/// A JSON object with extracted GTS IDs and all GTS references in the tree.
/// </summary>
public sealed class GtsJsonEntity
{
    /// <summary>Parsed GTS ID if the entity has a valid GTS identifier; null for anonymous instances.</summary>
    public GtsId? GtsId { get; }

    /// <summary>Schema/type ID (may be derived from entity ID chain or from schema fields).</summary>
    public string SchemaId { get; }

    /// <summary>Property name from which the entity ID was taken, or null.</summary>
    public string? SelectedEntityField { get; }

    /// <summary>Property name from which the schema ID was taken or derived, or null.</summary>
    public string? SelectedSchemaIdField { get; }

    /// <summary>True if the document is treated as a JSON Schema (has $schema).</summary>
    public bool IsSchema { get; }

    /// <summary>The source JSON object.</summary>
    public JsonObject Content { get; }

    /// <summary>All GTS ID references found in the tree, with their paths.</summary>
    public IReadOnlyList<GtsReference> GtsRefs { get; }

    /// <summary>Display label (e.g. GTS ID or file name).</summary>
    public string Label { get; }

    internal GtsJsonEntity(
        GtsId? gtsId,
        string schemaId,
        string? selectedEntityField,
        string? selectedSchemaIdField,
        bool isSchema,
        JsonObject content,
        IReadOnlyList<GtsReference> gtsRefs,
        string label)
    {
        GtsId = gtsId;
        SchemaId = schemaId;
        SelectedEntityField = selectedEntityField;
        SelectedSchemaIdField = selectedSchemaIdField;
        IsSchema = isSchema;
        Content = content;
        GtsRefs = gtsRefs;
        Label = label;
    }
}
