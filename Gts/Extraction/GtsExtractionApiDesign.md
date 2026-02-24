# OP#2 – ID Extraction API design (.NET)

Idiomatic C# API for extracting GTS identifiers from JSON, using `System.Text.Json` and `JsonObject` instead of `map[string]any`.

---

## 1. Types

### 1.1 Config

```csharp
namespace Gts.Extraction;

/// <summary>Configuration for which JSON property names to use when extracting entity and schema IDs.</summary>
public sealed class GtsExtractionOptions
{
    public static GtsExtractionOptions Default { get; } = new();

    public IReadOnlyList<string> EntityIdPropertyNames { get; init; } = new[]
    {
        "$id", "gtsId", "gtsIid", "gtsOid", "gtsI",
        "gts_id", "gts_oid", "gts_iid", "id"
    };

    public IReadOnlyList<string> SchemaIdPropertyNames { get; init; } = new[]
    {
        "gtsTid", "gtsType", "gtsT", "gts_t", "gts_tid", "gts_type",
        "type", "schema"
    };
}
```

- **Naming**: `GtsExtractionOptions` (or `GtsExtractionConfig`) to avoid confusion with `JsonSerializerOptions`.
- **Immutability**: Init-only properties; `Default` is a single shared instance.
- **Overrides**: Callers can `new GtsExtractionOptions { EntityIdPropertyNames = new[] { "customId", "id" } }`.

---

### 1.2 Extract result (lightweight)

```csharp
namespace Gts.Extraction;

/// <summary>Result of extracting the primary entity/schema ID from a JSON object.</summary>
public sealed record ExtractIdResult(
    string Id,
    string? SchemaId,
    string? SelectedEntityField,
    string? SelectedSchemaIdField,
    bool IsSchema
);
```

- **Record**: Immutable, value-style result; matches “extract once, use result”.
- **Nulls**: `Id` is never null (empty string when nothing found). Nullable for optional fields.

---

### 1.3 GTS reference (path + ID)

```csharp
namespace Gts.Extraction;

/// <summary>A GTS ID found somewhere in a JSON tree, with its JSON path.</summary>
public sealed record GtsReference(string Id, string SourcePath);
```

- **Path**: Same semantics as gts-go (e.g. `"$id"`, `"properties.x.$ref"`, `"items[0].$ref"`).

---

### 1.4 Rich entity (optional, for full extraction + refs)

```csharp
namespace Gts.Extraction;

/// <summary>JSON object with extracted GTS IDs and all GTS references in the tree.</summary>
public sealed class GtsJsonEntity
{
    public GtsId? GtsId { get; }
    public string SchemaId { get; }
    public string? SelectedEntityField { get; }
    public string? SelectedSchemaIdField { get; }
    public bool IsSchema { get; }
    public JsonObject Content { get; }
    public IReadOnlyList<GtsReference> GtsRefs { get; }
    public string Label { get; }

    internal GtsJsonEntity(/* ... */) { }
}
```

- **Content**: Keep as `JsonObject` so callers can continue to work with the same document.
- **Label**: Optional; can be path, filename, or `GtsId.Id` for display (mirrors gts-go).

---

## 2. Entry points

### 2.1 Primary: extract from JsonObject

```csharp
namespace Gts.Extraction;

public static class GtsExtractor
{
    /// <summary>Extracts the primary entity/schema ID from a JSON object.</summary>
    /// <param name="json">Root JSON object (e.g. from JsonDocument.RootElement or parsed as JsonObject).</param>
    /// <param name="options">Optional; uses default entity/schema property names if null.</param>
    /// <returns>ExtractIdResult with Id, SchemaId, which fields were used, and IsSchema.</returns>
    public static ExtractIdResult ExtractId(
        JsonObject json,
        GtsExtractionOptions? options = null)
    {
        options ??= GtsExtractionOptions.Default;
        // ... implementation
    }
}
```

- **JsonObject**: Requires a JSON object; avoids “content is array/string” ambiguity. Callers get it from `JsonNode.AsObject()`, or from `JsonDocument.RootElement` via a small helper (see overloads below).
- **Options**: Optional; null = use default property names.

---

### 2.2 Overload: accept JsonNode (object only)

```csharp
/// <summary>Extracts ID if the root is a JSON object; otherwise returns a result with empty Id.</summary>
public static ExtractIdResult ExtractId(
    JsonNode? node,
    GtsExtractionOptions? options = null)
{
    if (node is JsonObject obj)
        return ExtractId(obj, options);
    return new ExtractIdResult("", null, null, null, false);
}
```

- **JsonNode**: Convenience for “I have a parsed JSON value”; only objects are processed.

---

### 2.3 Overload: accept JsonElement (from JsonDocument)

```csharp
/// <summary>Extracts ID from a JSON object element.</summary>
/// <exception cref="ArgumentException">Thrown if element is not ValueKind.Object.</exception>
public static ExtractIdResult ExtractId(
    JsonElement element,
    GtsExtractionOptions? options = null)
{
    if (element.ValueKind != JsonValueKind.Object)
        throw new ArgumentException("JSON root must be an object.", nameof(element));
    using var doc = JsonDocument.ParseValue(element.GetRawValue());
    var root = doc.RootElement;
    var obj = JsonObject.Create(root); // or manual walk
    return ExtractId(obj!, options);
}
```

- **JsonElement**: For pipelines that use `JsonDocument` and don’t want to round-trip through `JsonNode`. Note: `JsonObject.Create(JsonElement)` exists in .NET 9+; for net8.0 we can use a small adapter that builds a `JsonObject` from `JsonElement` or keep an overload that takes a `JsonDocument` and uses `RootElement` with a custom reader.

---

### 2.4 Rich extraction: entity + all references

```csharp
/// <summary>Builds a GtsJsonEntity with primary ID and all GTS references in the tree.</summary>
public static GtsJsonEntity ExtractEntity(
    JsonObject json,
    GtsExtractionOptions? options = null)
{
    options ??= GtsExtractionOptions.Default;
    // Same logic as gts-go NewJsonEntity + extractGtsReferences
}
```

- **Single call**: One place to get “main ID + schema ID + every GTS ref with path”.

---

### 2.5 Collect all GTS references from any JSON

```csharp
/// <summary>Walks the JSON tree and returns every string that is a valid GTS ID, with path.</summary>
public static IReadOnlyList<GtsReference> ExtractReferences(JsonNode? node)
{
    var refs = new List<GtsReference>();
    var seen = new HashSet<string>();
    WalkAndCollectRefs(node, "", refs, seen);
    return refs;
}
```

- **JsonNode**: Natural for recursive walk (object/array/value). For `JsonElement`-only callers, we could add an overload that walks `JsonElement` and returns paths + IDs.

---

## 3. Helpers (internal or public)

- **GetStringFromObject(JsonObject obj, string propertyName)**: Return trimmed string or null; for `"$id"` strip `"gts://"` prefix only.
- **IsJsonSchema(JsonObject obj)**: True if `$schema` or `$$schema` exists (same rule as gts-go).
- **Valid GTS ID**: Use `GtsId.TryParse(s, out _)` (and optionally `TryParsePattern` for patterns); no separate `IsValidGtsID` unless we add an extension.

---

## 4. Usage examples

```csharp
using System.Text.Json;
using System.Text.Json.Nodes;
using Gts.Extraction;

// From JsonNode (e.g. JsonNode.Parse)
var node = JsonNode.Parse("""{ "gtsId": "gts.acme.order.ns.invoice.v1.0", "name": "Order 1" }""");
var result = GtsExtractor.ExtractId(node!.AsObject());
Console.WriteLine(result.Id);           // gts.acme.order.ns.invoice.v1.0
Console.WriteLine(result.IsSchema);     // false

// From JsonDocument
using var doc = JsonDocument.Parse(jsonString);
var fromDoc = GtsExtractor.ExtractId(doc.RootElement);

// Custom property names
var options = new GtsExtractionOptions
{
    EntityIdPropertyNames = new[] { "customId", "id" }
};
var custom = GtsExtractor.ExtractId(obj, options);

// Full entity + all references
var entity = GtsExtractor.ExtractEntity(obj);
Console.WriteLine(entity.GtsId?.Id);
foreach (var ref in entity.GtsRefs)
    Console.WriteLine($"{ref.SourcePath} -> {ref.Id}");

// Only collect references
var refs = GtsExtractor.ExtractReferences(node);
```

---

## 5. Assembly and namespace

- **Namespace**: `Gts.Extraction` keeps extraction types and `GtsExtractor` grouped; `Gts` already has `GtsId`, `GtsIdSegment`.
- **Project**: Same `Gts` assembly; no extra dependency. Only add `System.Text.Json` if not already referenced (net8.0 has it in the shared framework).

---

## 6. Summary

| Concept            | Type                     | Notes                                      |
|--------------------|--------------------------|--------------------------------------------|
| Config             | `GtsExtractionOptions`   | Init-only lists, `Default` static           |
| Lightweight result | `ExtractIdResult` record | Id, SchemaId?, fields, IsSchema             |
| One ref            | `GtsReference` record    | Id + SourcePath                            |
| Full entity        | `GtsJsonEntity` class    | GtsId?, SchemaID, Content, GtsRefs, Label   |
| Extract ID         | `GtsExtractor.ExtractId(JsonObject, options?)` | Primary API                        |
| Extract entity     | `GtsExtractor.ExtractEntity(JsonObject, options?)` | ID + refs                        |
| Extract refs only  | `GtsExtractor.ExtractReferences(JsonNode)`    | Walk tree, return refs             |
| Input              | `JsonObject` / `JsonNode` / `JsonElement`     | Overloads for different sources  |

This keeps the API close to gts-go behavior while using `JsonObject`/`JsonNode` idiomatically in .NET.
