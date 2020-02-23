using System.Collections.Generic;
using System.Linq;
using ParserObjects;
using ParserObjects.Parsers;
using Scoop.Parsing.Tokenization.Parsers;
using static ParserObjects.Parsers.ParserMethods;
using static Scoop.Parsing.Tokenization.Parsers.TokenParserMethods;

namespace Scoop.Parsing.Tokenization
{
    public static class LexicalGrammar
    {
        private static readonly HashSet<char> _hexDigits = new HashSet<char>("abcdefABCDEF0123456789");

        public static IParser<char, Token> GetParser()
        {
            // TODO: Cache this, we don't need to re-initialize it over and over again
            var numbers = BuildNumberParser();

            var wordMaybeAt = First(
                Match("@", c => "@"),
                Produce<char, string>(() => "")
            );
            var wordStartChar = Match<char>(c => char.IsLetter(c) || c == '_');
            var wordBodyChar = Match<char>(c => char.IsLetter(c) || char.IsDigit(c) || c == '_');

            var words = Rule(
                wordMaybeAt,
                wordStartChar,
                wordBodyChar.List().Transform(c => c.ToArray()),

                (prefix, start, rest) => new Token(prefix + start + new string(rest), TokenType.Word)
            );

            var chars = BuildCharacterLiteralParser();

            var notNewlineChar = Match<char>(c => c != '\r' && c != '\n');

            var singleLineComments = Rule(
                    Match("//", c => new string(c)),
                    notNewlineChar.List().Transform(l => new string(l.ToArray())),

                    (prefix, content) => prefix + content
                )
                .Named("SingleLineComment");

            var multiLineComments = new MultilineCommentParser();

            var cSharpLiterals = new CSharpLiteralParser();

            var operators = new OperatorParser();

            var strings = new StringParser();

            var allTokens = First(
                End<char>().Transform(x => Token.EndOfInput()),
                cSharpLiterals,
                words,
                operators,
                numbers,
                strings,
                chars,
                Produce<char, Token>(t => new Token(t.GetNext().ToString(), TokenType.Unknown))
            );

            var whitespace = Match<char>(char.IsWhiteSpace).ListCharToString(true);
            var whitespaceOrComment = First(
                whitespace,
                singleLineComments,
                multiLineComments
            );

            return Rule(
                // TODO: Get a list of all whitespace and comments, and include those in the Token we return
                whitespaceOrComment.List(),
                Produce<char, Location>(t => t.CurrentLocation),
                allTokens,

                (ws, location, token) =>
                {
                    token.Location = location;
                    token.Frontmatter = ws.ToList();
                    // TODO: Should we keep track of the leading whitespace at all?
                    return token;
                }
            );
        }

        private static IParser<char, Token> BuildCharacterLiteralParser()
        {
            var hexDigits = Match<char>(c => _hexDigits.Contains(c));

            var hexCharLiteral = Rule(
                Match<char>(c => c == '\\'),
                Match<char>(c => c == 'x'),
                // TODO: 1-4 of these only
                hexDigits.ListCharToString(),

                (slash, x, c) => "\\x" + c
            );

            var unicodeCharLiteral = Rule(
                Match<char>(c => c == '\\'),
                Match<char>(c => c == 'u'),
                // TODO: 1-4 of these or exactly-4 of these?
                hexDigits.ListCharToString(),

                (slash, u, c) => "\\u" + c
            );

            var charEscape = Rule(
                Match<char>(c => c == '\\'),
                Match<char>(c => "abfnrtv\\'\"0".Contains(c)),

                (slash, c) => "\\" + c
            );

            var chars = Rule(
                Match<char>(c => c == '\''),
                First(
                    hexCharLiteral,
                    unicodeCharLiteral,
                    charEscape,
                    Transform(Match<char>(c => c != '\0' && c != '\''), c => c.ToString()),
                    Produce<char, string>(() => (string) null)
                ),
                First(
                    Match<char>(c => c == '\''),
                    Produce<char, char>(t => '\0')
                ),

                (start, content, end) =>
                {
                    if (string.IsNullOrEmpty(content))
                        return Token.Character("").WithDiagnostics(Errors.UnrecognizedCharLiteral);
                    var token = Token.Character("'" + content + "'");
                    if (end == '\0')
                        token.WithDiagnostics(Errors.MissingClosedSingleQuote);
                    return token;
                }
            );
            return chars;
        }

        private static IParser<char, Token> BuildNumberParser()
        {
            var hexDigits = Match<char>(c => _hexDigits.Contains(c));
            var digits = Match<char>(char.IsDigit);

            // "0x" <hexDigit> (("_" <hexDigit>) | <hexDigit>)*
            var hexNumber = Rule(
                Match("0x", c => c),
                hexDigits,
                First(
                        Rule(
                            Match<char>(c => c == '_'),
                            hexDigits,

                            (sep, digit) => new[] { sep, digit }
                        ),
                        digits.Transform(c => new[] { c })
                    )
                    .List().Transform(x => new string(x.SelectMany(y => y).ToArray())),
                First(
                    Match("UL", c => TokenType.ULong),
                    Match("U", c => TokenType.UInteger),
                    Match("L", c => TokenType.Long),
                    Produce<char, TokenType>(() => TokenType.Integer)
                ),

                (prefix, first, rest, type) => new Token(int.Parse(first + rest, System.Globalization.NumberStyles.HexNumber).ToString(), type)
            );

            // <digit> (("_" <digit>) | <digit>)*
            var digitList = Rule(
                digits,
                First(
                        Rule(
                            Match<char>(c => c == '_'),
                            digits,

                            (sep, digit) => new[] { sep, digit }
                        ),
                        digits.Transform(c => new[] { c })
                    )
                    .List().Transform(x => x.SelectMany(y => y)),

                (first, rest) => first + new string(rest.ToArray())
            );

            // "-"? <digitList> "." <digitList>
            var decimalNumber = Rule(
                Optional(Match("-", c => "-"), () => ""),
                digitList,
                Match(".", c => "."),
                digitList,
                First(
                    Match("F", c => TokenType.Float),
                    Match("M", c => TokenType.Decimal),
                    Produce<char, TokenType>(() => TokenType.Double)
                ),

                (neg, whole, dot, fract, type) => new Token(neg + whole + "." + fract, type)
            );

            // "-"? <digitList>
            var integralNumber = Rule(
                Optional(Match("-", c => "-"), () => ""),
                digitList,
                First(
                    Match("UL", c => TokenType.ULong),
                    Match("U", c => TokenType.UInteger),
                    Match("L", c => TokenType.Long),
                    Produce<char, TokenType>(() => TokenType.Integer)
                ),

                (neg, whole, type) => new Token(neg + whole, type)
            );

            var numbers = First(
                hexNumber,
                decimalNumber,
                integralNumber
            );
            return numbers;
        }
    }
}