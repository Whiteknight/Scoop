namespace Scoop.Tokenization
{
    public enum TokenType
    {
        EndOfInput,

        Whitespace,
        Comment,
        Identifier,
        Keyword,
        Operator,
        String,
        Character,

        Integer,
        UInteger,
        Long,
        ULong,
        Float,
        Double,
        Decimal,

        CSharpLiteral
    }
}