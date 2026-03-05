namespace Gts.Parsing;

public sealed class ParseResult
{
    public static readonly ParseResult Success = new();
    
    public static implicit operator bool(ParseResult result)
    {
        return result == Success;
    }
}
