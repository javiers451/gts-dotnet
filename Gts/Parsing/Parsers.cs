using System.Collections;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace Gts.Parsing;

internal static class Parsers
{
    internal static readonly Parser<char, char> Dot =
        Char('.').Labelled(".");
    internal static readonly Parser<char, char> Tilde =
        Char('~').Labelled("~");

    internal static readonly Parser<char, string> Wildcard =
        String("*").Labelled(nameof(Wildcard));
    
    internal static readonly Parser<char, char> V =
        Char('v').Labelled(nameof(V));

    internal static readonly Parser<char, int> VersionNumber =
        Num.Labelled(nameof(VersionNumber));
    internal static readonly Parser<char, int> VersionMajor =
        V.Then(VersionNumber).Labelled(nameof(VersionMajor));
    internal static readonly Parser<char, int> VersionMinor =
        VersionNumber.Labelled(nameof(VersionMinor));

    internal static readonly Parser<char, VersionInfo> VersionFull =
        VersionMajor.Then(
            Dot.Then(VersionMinor).Optional(),
            (major, minor) => new VersionInfo(major, minor.HasValue ? minor.Value : null)
        )
        .Labelled(nameof(VersionMajor));

    internal static readonly Parser<char, string> VersionFullString =
        VersionFull.Select(v => v.ToString());

    // internal static readonly Parser<char, VersionInfo?> VersionSuffix =
    //     Dot.Then(VersionFull)
    //     .Optional().Select(v => v.HasValue ? (VersionInfo?)v.Value : null);

    private static readonly Parser<char, char> IdentifierStart =
        Token(c => c == '_' || c is >= 'a' and <= 'z');

    private static readonly Parser<char, char> IdentifierRest =
        Token(c => c == '_' || c is >= 'a' and <= 'z' || c is >= '0' and <= '9');

    internal static readonly Parser<char, string> Identifier =
        IdentifierStart.Then(IdentifierRest.Many(), (first, rest) => first + string.Concat(rest))
            .Labelled(nameof(Identifier));

    internal static readonly Parser<char, string> IdentifierOrWildcard =
        Wildcard.Or(Identifier)
            .Labelled(nameof(Identifier));
    
    internal static readonly Parser<char, string> GtsPrefix =
        String("gts").Labelled(nameof(GtsPrefix));
    
    internal static readonly Parser<char, string> Vendor =
        Identifier.Labelled(nameof(Vendor));
    internal static readonly Parser<char, string> Package =
        Identifier.Labelled(nameof(Package));
    internal static readonly Parser<char, string> Namespace =
        Identifier.Labelled(nameof(Namespace));
    internal static readonly Parser<char, string> Type =
        Identifier.Labelled(nameof(Type));
    
    internal static readonly Parser<char, SegmentInfo> Segment =
        Vendor
            .Then(Dot.Then(Package), (vendor, package) => (vendor, package))
            .Then(Dot.Then(Namespace), (t, nspace) => (t.vendor, t.package, nspace))
            .Then(Dot.Then(Type), (t, type) => (t.vendor, t.package, t.nspace, type))
            .Then(Dot.Then(VersionFull),
                (t, version) => new SegmentInfo(
                    t.vendor,
                    t.package,
                    t.nspace,
                    t.type,
                    version))
            .Labelled(nameof(Segment));
    
    internal static readonly Parser<char, SegmentInfo> Pattern =
        Vendor
            .Then(Try(Dot.Then(Package)).Optional(), (vendor, package) => (vendor, package))
            .Then(Try(Dot.Then(Namespace)).Optional(), (t, nspace) => (t.vendor, t.package, nspace))
            .Then(Try(Dot.Then(Type)).Optional(), (t, type) => (t.vendor, t.package, t.nspace, type))
            .Then(Dot.Then(Wildcard.Or(VersionFullString)).Optional(),
                (t, version) => new SegmentInfo(
                    t.vendor,
                    t.package.GetValueOrDefault(),
                    t.nspace.GetValueOrDefault(),
                    t.type.GetValueOrDefault(),
                    version.GetValueOrDefault() is "*" or null
                        ? null
                        : VersionInfo.FromString(version.Value)))
            .Labelled(nameof(Pattern));

    // internal static readonly Parser<char, TypeInfo> TypeSuffix =
    //     Try(Tilde.Then(Segment)).AtLeastOnce().Select(segments => new TypeInfo(segments))
    //         .Or(Tilde.Select(_ => new TypeInfo([])));

    // internal static readonly Parser<char, InstanceInfo> InstanceSuffix =
    //     Tilde.Then(Segment)
    //         .Then(Tilde.Then(Segment).Many(), (head, tail) => new InstanceInfo(head, tail));

    internal static readonly Parser<char, IdentifierInfo> GtsTypeId =
        GtsPrefix.Then(Dot)
            .Then(Segment.SeparatedAndTerminatedAtLeastOnce(Tilde),
                (_, segments) => new IdentifierInfo(IdentifierKind.Type, segments));

    internal static readonly Parser<char, IdentifierInfo> GtsInstanceId =
        GtsPrefix.Then(Dot)
            .Then(Segment.SeparatedAtLeastOnce(Tilde),
                (_, segments) => new IdentifierInfo(IdentifierKind.Instance, segments));
    
    internal static readonly Parser<char, IdentifierInfo> GtsPattern =
        GtsPrefix.Then(Dot)
            .Then(Pattern.SeparatedAtLeastOnce(Tilde),
                (_, segments) => new IdentifierInfo(IdentifierKind.Pattern, segments));

    internal record struct VersionInfo(
        int Major,
        int? Minor)
    {
        public override string ToString() => Minor == null
            ? $"v{Major}"
            : $"v{Major}.{Minor}";

        public static VersionInfo? FromString(string str)
        {
            var parts = str.Substring(1).Split('.');
            return parts.Length == 1
                ? new VersionInfo(int.Parse(parts[0]), null)
                : new VersionInfo(int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }

    internal record struct SegmentInfo(
        string? Vendor,
        string? Package,
        string? Namespace,
        string? Type,
        VersionInfo? Version);

    internal enum IdentifierKind
    {
        Type,
        Instance,
        Pattern,
    }
    
    internal record struct IdentifierInfo(
        IdentifierKind Kind, IEnumerable<SegmentInfo> Segments) : IEnumerable<SegmentInfo>
    {
        public bool IsType => Kind == IdentifierKind.Type;
        public bool IsInstance => Kind == IdentifierKind.Instance;
        public bool IsPattern => Kind == IdentifierKind.Pattern;
        
        public IEnumerator<SegmentInfo> GetEnumerator() => Segments.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
