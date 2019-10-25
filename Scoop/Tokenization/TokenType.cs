namespace Scoop.Tokenization
{
    public enum TokenType
    {
        Unknown,
        EndOfInput,

        Whitespace,
        Comment,

        // Word can be either Identifier or Keyword, depending on what is needed
        Word,
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