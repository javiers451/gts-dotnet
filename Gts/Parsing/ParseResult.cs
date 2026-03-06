namespace Gts.Parsing;

public sealed class ParseResult
{
    public static readonly ParseResult Success = new();
    public static readonly ParseResult ArgumentIsNull = new(); // TODO: actual error message
    
    public static implicit operator bool(ParseResult result)
    {
        return result == Success;
    }
}
