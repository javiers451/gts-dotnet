using System.Text.Json;
using System.Text.Json.Nodes;

namespace Gts.Extraction;

/// <summary>
/// ID Extraction: extract GTS identifiers from JSON objects using System.Text.Json.
/// </summary>
public static class GtsExtract
{
    private const string GtsUriPrefix = "gts://";

    /// <summary>
    /// Extracts the primary entity/schema ID from a JSON object.
    /// </summary>
    /// <param name="json">Root JSON object.</param>
    /// <param name="options">Optional; uses default entity/schema property names if null.</param>
    /// <returns>ExtractIdResult with Id, SchemaId, which fields were used, and IsSchema.</returns>
    public static ExtractResult ExtractId(JsonObject json, GtsExtractOptions? options = null)
    {
        options ??= GtsExtractOptions.Default;
        var entity = ExtractEntityInternal(json, options);

        string id;
        if (entity.IsSchema || entity.GtsId != null)
        {
            id = entity.GtsId?.Id ?? "";
        }
        else
        {
            // Anonymous instance: use value from selected entity field (GetFieldValue trims; strips gts:// only for $id)
            id = entity.SelectedEntityField != null ? GetFieldValue(json, entity.SelectedEntityField) ?? "" : "";
        }

        return new ExtractResult(
            Id: id,
            SchemaId: string.IsNullOrEmpty(entity.SchemaId) ? null : entity.SchemaId,
            SelectedEntityField: entity.SelectedEntityField,
            SelectedSchemaIdField: entity.SelectedSchemaIdField,
            entity.IsSchema);
    }

    /// <summary>
    /// Extracts ID if the root is a JSON object; otherwise returns a result with empty Id.
    /// </summary>
    public static ExtractResult ExtractId(JsonNode? node, GtsExtractOptions? options = null)
    {
        if (node is JsonObject obj)
            return ExtractId(obj, options);
        
        return new ExtractResult("", null, null, null, false);
    }

    /// <summary>
    /// Extracts ID from a JSON object element (e.g. from JsonDocument.RootElement).
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when element is not ValueKind.Object.</exception>
    public static ExtractResult ExtractId(JsonElement element, GtsExtractOptions? options = null)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("JSON root must be an object.", nameof(element));
        
        var node = JsonNode.Parse(element.GetRawText());
        if (node is JsonObject obj)
            return ExtractId(obj, options);
        
        return new ExtractResult("", null, null, null, false);
    }

    /// <summary>
    /// Builds a GtsJsonEntity with primary ID, schema ID, and all GTS references in the tree.
    /// </summary>
    public static GtsJsonEntity ExtractEntity(JsonObject json, GtsExtractOptions? options = null)
    {
        options ??= GtsExtractOptions.Default;
        
        return ExtractEntityInternal(json, options);
    }

    /// <summary>
    /// Walks the JSON tree and returns every string that is a valid GTS ID, with its path.
    /// </summary>
    public static IReadOnlyList<GtsReference> ExtractReferences(JsonNode? node)
    {
        var refs = new List<GtsReference>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        WalkAndCollectRefs(node, "", refs, seen);
        return refs;
    }

    private static GtsJsonEntity ExtractEntityInternal(JsonObject json, GtsExtractOptions options)
    {
        var isSchema = IsJsonSchema(json);
        var (selectedEntityField, entityIdValue) = FirstNonEmptyField(json, options.EntityIdPropertyNames);

        string schemaId;
        string? selectedSchemaIdField = null;

        if (isSchema)
        {
            // Derived schema: chain has more than one ~
            if (!string.IsNullOrEmpty(entityIdValue) && IsValidGtsId(entityIdValue) && entityIdValue.EndsWith('~'))
            {
                var firstTilde = entityIdValue.IndexOf('~');
                if (firstTilde > 0)
                {
                    var afterFirst = entityIdValue.AsSpan(firstTilde + 1);
                    var secondTilde = afterFirst.IndexOf('~');
                    if (secondTilde >= 0)
                    {
                        selectedSchemaIdField = selectedEntityField;
                        schemaId = entityIdValue[..(firstTilde + 1)];
                        return BuildEntity(json, options, isSchema, entityIdValue, selectedEntityField, schemaId, selectedSchemaIdField);
                    }
                }
            }
            var schemaValue = GetFieldValue(json, "$schema");
            if (!string.IsNullOrEmpty(schemaValue))
            {
                selectedSchemaIdField = "$schema";
                return BuildEntity(json, options, isSchema, entityIdValue, selectedEntityField, schemaValue, selectedSchemaIdField);
            }
            return BuildEntity(json, options, isSchema, entityIdValue, selectedEntityField, "", null);
        }
        else
        {
            // Instance: try entity ID chain first
            if (!string.IsNullOrEmpty(entityIdValue) && IsValidGtsId(entityIdValue) && !entityIdValue.EndsWith('~'))
            {
                var lastTilde = entityIdValue.LastIndexOf('~');
                if (lastTilde > 0)
                {
                    selectedSchemaIdField = selectedEntityField;
                    schemaId = entityIdValue[..(lastTilde + 1)];
                    return BuildEntity(json, options, isSchema, entityIdValue, selectedEntityField, schemaId, selectedSchemaIdField);
                }
            }
            var (schemaField, schemaValue) = FirstNonEmptyField(json, options.SchemaIdPropertyNames);
            schemaId = schemaValue ?? "";
            selectedSchemaIdField = string.IsNullOrEmpty(schemaValue) ? null : schemaField;
            return BuildEntity(json, options, isSchema, entityIdValue, selectedEntityField, schemaId, selectedSchemaIdField);
        }
    }

    private static GtsJsonEntity BuildEntity(
        JsonObject json,
        GtsExtractOptions options,
        bool isSchema,
        string? entityIdValue,
        string? selectedEntityField,
        string schemaId,
        string? selectedSchemaIdField)
    {
        GtsId? gtsId = null;
        if (isSchema)
        {
            if (!string.IsNullOrEmpty(entityIdValue) && IsValidGtsId(entityIdValue) && GtsId.TryParse(entityIdValue, out var parsed))
                gtsId = parsed;
        }
        else
        {
            if (!string.IsNullOrEmpty(entityIdValue) && IsValidGtsId(entityIdValue) && GtsId.TryParse(entityIdValue, out var parsed))
            {
                gtsId = parsed;
                if (string.IsNullOrEmpty(schemaId) && !string.IsNullOrEmpty(selectedEntityField))
                {
                    if (!entityIdValue.EndsWith('~'))
                    {
                        var lastTilde = entityIdValue.LastIndexOf('~');
                        if (lastTilde > 0)
                        {
                            schemaId = entityIdValue[..(lastTilde + 1)];
                            selectedSchemaIdField = selectedEntityField;
                        }
                    }
                }
            }
        }

        var refs = ExtractReferences(json);
        var label = gtsId != null ? gtsId.Id : "";

        return new GtsJsonEntity(
            gtsId,
            schemaId,
            selectedEntityField,
            selectedSchemaIdField,
            isSchema,
            json,
            refs,
            label);
    }

    private static bool IsJsonSchema(JsonObject json)
    {
        if (json.TryGetPropertyValue("$schema", out _)) return true;
        if (json.TryGetPropertyValue("$$schema", out _)) return true;
        return false;
    }

    private static string? GetFieldValue(JsonObject json, string propertyName)
    {
        if (!json.TryGetPropertyValue(propertyName, out var node))
            return null;
        
        var s = node?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(s))
            return null;
        
        var trimmed = s!.Trim();
        if (trimmed.Length == 0)
            return null;
        
        if (propertyName == "$id" && trimmed.StartsWith(GtsUriPrefix, StringComparison.Ordinal))
            trimmed = trimmed.Substring(GtsUriPrefix.Length);
        
        return trimmed;
    }

    private static (string? field, string? value) FirstNonEmptyField(JsonObject json, IReadOnlyList<string> propertyNames)
    {
        foreach (var name in propertyNames)
        {
            var val = GetFieldValue(json, name);
            if (!string.IsNullOrEmpty(val) && IsValidGtsId(val))
                return (name, val);
        }
        
        foreach (var name in propertyNames)
        {
            var val = GetFieldValue(json, name);
            if (!string.IsNullOrEmpty(val))
                return (name, val);
        }
        
        return (null, null);
    }

    private static bool IsValidGtsId(string s)
    {
        return GtsId.TryParse(s, out _) || GtsId.TryParsePattern(s, out _);
    }

    private static void WalkAndCollectRefs(JsonNode? node, string path, List<GtsReference> refs, HashSet<string> seen)
    {
        if (node == null) return;

        if (node is JsonValue value)
        {
            var str = value.GetValue<string>();
            
            if (!string.IsNullOrEmpty(str) && IsValidGtsId(str))
            {
                var sourcePath = string.IsNullOrEmpty(path) ? "root" : path;
                var key = str + "|" + sourcePath;
                if (seen.Add(key))
                    refs.Add(new GtsReference(str, sourcePath));
            }
            
            return;
        }

        if (node is JsonObject obj)
        {
            foreach (var (k, v) in obj)
            {
                var nextPath = string.IsNullOrEmpty(path) ? k : path + "." + k;
                WalkAndCollectRefs(v, nextPath, refs, seen);
            }
            
            return;
        }

        if (node is JsonArray arr)
        {
            for (var i = 0; i < arr.Count; i++)
            {
                var nextPath = string.IsNullOrEmpty(path) ? "[" + i + "]" : path + "[" + i + "]";
                WalkAndCollectRefs(arr[i], nextPath, refs, seen);
            }
        }
    }
}
