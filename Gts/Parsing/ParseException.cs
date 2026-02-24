namespace Gts.Parsing;

public class ParseException : Exception
{
    public ParseResult ParseResult { get; }

    public ParseException(ParseResult parseResult)
    {
        ParseResult = parseResult;
    }
}