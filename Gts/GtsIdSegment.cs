namespace Gts;

/// <summary>
/// Represents a parsed segment of a GTS identifier.
/// </summary>
public sealed class GtsIdSegment
{
    internal GtsIdSegment(
        //string? segment,
        string? vendor,
        string? package,
        string? ns,
        string? type,
        int? versionMajor,
        int? versionMinor,
        bool isType,
        bool isWildcard)
    {
        //Segment = segment;
        Vendor = vendor;
        Package = package;
        Namespace = ns;
        Type = type;
        VersionMajor = versionMajor;
        VersionMinor = versionMinor;
        IsType = isType;
        IsWildcard = isWildcard;
    }

    internal GtsIdSegment()
    {
    }
    
    //public string? Segment { get; internal set; }
    
    public string? Vendor { get; internal set; }
    
    public string? Package { get; internal set; }
    
    public string? Namespace { get; internal set; }
    
    public string? Type { get; internal set; }
    
    public int? VersionMajor { get; internal set; }
    
    public int? VersionMinor { get; internal set; }
    
    public bool IsType { get; internal set; }
    
    public bool IsWildcard { get; internal set; }
}
