using System.Text;

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

    /// <summary>
    /// Returns the segment in GTS segment form.
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder(
            Vendor?.Length ?? 0 + Package?.Length ?? 0 + Namespace?.Length + Type?.Length ?? 0
                + (VersionMajor.HasValue ? 2 : 0) // rough estimation for version
                + (VersionMinor.HasValue ? 2 : 0)
        );

        if (Vendor is not null)
        {
            sb.Append(Vendor);
            sb.Append('.');
        }

        if (Package is not null)
        {
            sb.Append(Package);
            sb.Append('.');
        }

        if (Namespace is not null)
        {
            sb.Append(Namespace);
            sb.Append('.');
        }

        if (Type is not null)
        {
            sb.Append(Type);
            sb.Append('.');
        }

        if (VersionMajor.HasValue)
        {
            sb.Append('v');
            sb.Append(VersionMajor.Value);
            
            if (VersionMinor.HasValue)
            {
                sb.Append('.');
                sb.Append(VersionMinor.Value);
            }
        }
        else
        {
            sb.Append('*');
        }

        return sb.ToString();
    }
}
