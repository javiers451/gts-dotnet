using Gts.Parsing;
using Gts.Utils;
using Pidgin;
using ParseException = Gts.Parsing.ParseException;

namespace Gts;

/// <summary>
/// A validated GTS identifier.
/// </summary>
public sealed class GtsId
{
    public const int MaxLength = 1024;
    
    /// <summary>
    /// The canonical identifier string (lowercase, trimmed).
    /// </summary>
    public string Id { get; }
    
    /// <summary>
    /// ID is type
    /// </summary>
    public bool IsType { get; private set; }
    
    /// <summary>
    /// ID is instance
    /// </summary>
    public bool IsInstance { get; private set; }
    
    /// <summary>
    /// ID is pattern
    /// </summary>
    public bool IsPattern { get; private set; }
    
    /// <summary>
    /// Parsed segments (vendor.package.namespace.type.version per segment).
    /// </summary>
    public IReadOnlyCollection<GtsIdSegment> Segments { get; }
    
    internal GtsId(string id, IReadOnlyCollection<GtsIdSegment> segments)
    {
        Id = id;
        Segments = segments;
    }
    
    public static GtsId Parse(string id)
    {
        var parseResult = TryParseInternal(id, out GtsId? result);
        
        if (parseResult)
        {
            return result!;
        }

        throw new ParseException(parseResult);
    }
    
    public static ParseResult TryParse(string id, out GtsId? result)
    {
        return TryParseInternal(id, out result);
    }

    public static GtsId ParsePattern(string pattern)
    {
        var parseResult = TryParsePatternInternal(pattern, out GtsId? result);
        
        if (parseResult)
        {
            return result!;
        }

        throw new ParseException(parseResult);
    }

    public static ParseResult TryParsePattern(string pattern, out GtsId? result)
    {
        return TryParsePatternInternal(pattern, out result);
    }

    private static ParseResult TryParseInternal(string id, out GtsId? result)
    {
        var (parseResult, isType) = id.EndsWith("~")
            ? (Parsers.GtsTypeId.Parse(id), true)
            : (Parsers.GtsInstanceId.Parse(id), false);

        if (!parseResult.Success)
        {
            // TODO: add errors to the result
            result = null;
            return new ParseResult();
        }

        var segments = parseResult.Value
            .Select(MapSegment);

        result = new GtsId(id, new List<GtsIdSegment>(segments))
        {
            IsType = isType,
            IsInstance = !isType,
            IsPattern = false
        };

        return ParseResult.Success;
    }

    private static ParseResult TryParsePatternInternal(string pattern, out GtsId? result)
    {
        var parseResult = Parsers.GtsPattern.Parse(pattern);
        
        if (!parseResult.Success)
        {
            // TODO: add errors to the result
            result = null;
            return new ParseResult();
        }

        var segments = parseResult.Value
            .Select(MapSegment);

        result = new GtsId(pattern, new List<GtsIdSegment>(segments))
        {
            IsPattern = true
        };

        return ParseResult.Success;
    }

    private static GtsIdSegment MapSegment(Parsers.SegmentInfo s)
    {
        return new GtsIdSegment(
            s.Vendor, s.Package, s.Namespace, s.Type, s.Version?.Major, s.Version?.Minor, true, false);
    }

    /// <summary>
    /// Returns true if this identifier matches the given pattern.
    /// Pattern may contain at most one trailing wildcard (*); matching is segment-by-segment.
    /// </summary>
    public bool Matches(GtsId pattern)
    {
        // TODO: this logic is probably flawed, check
        if (pattern is null) return false;

        if (!pattern.Id.Contains('*'))
            return MatchSegments(pattern.Segments, Segments);

        if (pattern.Id.Count(c => c == '*') > 1 || !pattern.Id.EndsWith('*'))
            return false;

        return MatchSegments(pattern.Segments, Segments);
    }

    /// <summary>
    /// Returns true if this identifier matches the given pattern string.
    /// Pattern may contain at most one trailing wildcard (*).
    /// </summary>
    public bool Matches(string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) return false;
        if (!TryParsePattern(pattern, out var patternId)) return false;
        return Matches(patternId!);
    }

    private static bool MatchSegments(
        IReadOnlyCollection<GtsIdSegment> patternSegs, IReadOnlyCollection<GtsIdSegment> candidateSegs)
    {
        if (patternSegs.Count > candidateSegs.Count) return false;

        var patternList = patternSegs as IList<GtsIdSegment> ?? patternSegs.ToList();
        var candidateList = candidateSegs as IList<GtsIdSegment> ?? candidateSegs.ToList();

        for (var i = 0; i < patternList.Count; i++)
        {
            var pSeg = patternList[i];
            var cSeg = candidateList[i];

            if (pSeg.IsWildcard)
            {
                if (!string.IsNullOrEmpty(pSeg.Vendor) && pSeg.Vendor != cSeg.Vendor) return false;
                if (!string.IsNullOrEmpty(pSeg.Package) && pSeg.Package != cSeg.Package) return false;
                if (!string.IsNullOrEmpty(pSeg.Namespace) && pSeg.Namespace != cSeg.Namespace) return false;
                if (!string.IsNullOrEmpty(pSeg.Type) && pSeg.Type != cSeg.Type) return false;
                if (pSeg.VersionMajor.HasValue && pSeg.VersionMajor != cSeg.VersionMajor) return false;
                if (pSeg.VersionMinor.HasValue && (cSeg.VersionMinor is null || pSeg.VersionMinor != cSeg.VersionMinor)) return false;
                if (pSeg.IsType && pSeg.IsType != cSeg.IsType) return false;
                return true;
            }

            if (pSeg.Vendor != cSeg.Vendor) return false;
            if (pSeg.Package != cSeg.Package) return false;
            if (pSeg.Namespace != cSeg.Namespace) return false;
            if (pSeg.Type != cSeg.Type) return false;
            if (pSeg.VersionMajor != cSeg.VersionMajor) return false;
            if (pSeg.VersionMinor.HasValue && (cSeg.VersionMinor is null || pSeg.VersionMinor != cSeg.VersionMinor)) return false;
            if (pSeg.IsType != cSeg.IsType) return false;
        }

        return true;
    }
    
    /// <summary>
    /// Generates a deterministic UUID v5 from this GTS identifier using the GTS namespace.
    /// </summary>
    public Guid ToGuid()
    {
        return GuidUtils.Create(GuidUtils.GtsNamespace, Id);
    }
}
