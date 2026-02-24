> Status: initial draft v0.1, not for production use

# GTS .NET Library

An idiomatic C#/.NET library for working with **GTS** ([Global Type System](https://github.com/gts-spec/gts-spec)) identifiers and JSON/JSON Schema artifacts.

## Roadmap

Featureset:

- [x] **OP#1 - ID Validation**: Verify identifier syntax using regex patterns
- [ ] **OP#2 - ID Extraction**: Fetch identifiers from JSON objects or JSON Schema documents
- [x] **OP#3 - ID Parsing**: Decompose identifiers into constituent parts (vendor, package, namespace, type, version, etc.)
- [x] **OP#4 - ID Pattern Matching**: Match identifiers against patterns containing wildcards
- [x] **OP#5 - ID to UUID Mapping**: Generate deterministic UUIDs from GTS identifiers
- [ ] **OP#6 - Instance Validation**: Validate object instances against their corresponding schemas
- [ ] **OP#7 - Relationship Resolution**: Load all schemas and instances, resolve inter-dependencies, and detect broken references
- [ ] **OP#8 - Compatibility Checking**: Verify that schemas with different MINOR versions are compatible
- [ ] **OP#8.1 - Backward compatibility checking**
- [ ] **OP#8.2 - Forward compatibility checking**
- [ ] **OP#8.3 - Full compatibility checking**
- [ ] **OP#9 - Version Casting**: Transform instances between compatible MINOR versions
- [ ] **OP#10 - Query Execution**: Filter identifier collections using the GTS query language
- [ ] **OP#11 - Attribute Access**: Retrieve property values and metadata using the attribute selector (`@`)
- [ ] **OP#12 - Schema Validation**: Validate schema against its precedent schema

## Installation

```bash
#TODO: NuGet packages
```

## Usage

### Library

```csharp
using Gts;
```

### OP#3 - ID Parsing

Decompose GTS identifiers into constituent parts (vendor, package, namespace, type, version, etc.).

- **Parse** a type ID (trailing `~`) or instance ID; **TryParse** for safe parsing without exceptions.
- **ParsePattern** / **TryParsePattern** for patterns that may end with a wildcard (`.*`).

```csharp
// Parsing
var id = GtsId.Parse("gts.acme.order.ns.invoice.v1~");

// Safe parsing
if (GtsId.TryParse("gts.vendor.pkg.ns.type.v1.0", out var id))
    // do something

// Pattern (for matching)
var pattern = GtsId.ParsePattern("gts.acme.order.*");

// Safe pattern parsing
if (GtsId.TryParsePattern("gts.acme.order.*", out var pattern))
	// do something
```

### OP#4 - ID Pattern Matching

Match identifiers against patterns containing wildcards.

- **Matches(GtsId pattern)** — match this ID against a parsed pattern.
- **Matches(string pattern)** — match this ID against a pattern string.

```csharp
var candidate = GtsId.Parse("gts.acme.order.ns.invoice.v1.0");

// Match against pattern (parsed)
var pattern = GtsId.ParsePattern("gts.acme.order.*");
candidate.Matches(pattern);  // true

// Match against pattern string
candidate.Matches("gts.acme.order.*");       // true
candidate.Matches("gts.acme.order.ns.*");    // true
candidate.Matches("gts.other.*");            // false

// Exact match (no wildcard)
candidate.Matches("gts.acme.order.ns.invoice.v1.0");  // true
```

### OP#5 - ID to UUID Mapping

Generate a deterministic UUID v5 from a GTS identifier. The same ID always yields the same UUID (RFC 4122, namespace + name hashed).

- **ToGuid()** — returns a `Guid` for this GTS ID using the standard GTS namespace.

```csharp
var id = GtsId.Parse("gts.acme.order.ns.invoice.v1.0");
Guid uuid = id.ToGuid();  // deterministic: same ID → same UUID every time
```

