using System.Collections.Generic;
using System.Linq;
using ParserObjects;
using Scoop.Parsing.Tokenization.Parsers;
using static ParserObjects.ParserMethods<char>;

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
                Match('@').Transform(c => "@"),
                Produce(() => "")
            );
            var wordStartChar = Match(c => char.IsLetter(c) || c == '_');
            var wordBodyChar = Match(c => char.IsLetter(c) || char.IsDigit(c) || c == '_');

            var words = Rule(
                wordMaybeAt,
                wordStartChar,
                wordBodyChar.List().Transform(c => c.ToArray()),

                (prefix, start, rest) => new Token(prefix + start + new string(rest), TokenType.Word)
            );

            var chars = BuildCharacterLiteralParser();

            var notNewlineChar = Match(c => c != '\r' && c != '\n');

            var singleLineComments = Rule(
                    Match("//").Transform(c => new string(c.ToArray())),
                    notNewlineChar.List().Transform(l => new string(l.ToArray())),

                    (prefix, content) => prefix + content
                )
                .Named("SingleLineComment");

            var multiLineComments = new MultilineCommentParser();

            var cSharpLiterals = new CSharpLiteralParser();

            var operators = Trie<string>(trie => trie
                    .AddMany(".", "?.", ",", ";")
                    .AddMany("(", ")", "{", "}", "[", "]")
                    .AddMany("+", "-", "/", "*", "&", "|", "^")
                    .AddMany("&&", "||")
                    .AddMany("~", "!")
                    .AddMany("++", "--")
                    .AddMany("=>")
                    .AddMany("??", "??=")
                    .AddMany("?", ":")
                    .AddMany("=", "+=", "-=", "*=", "/=", "%=", "&=", "^=", "|=")
                    .AddMany("==", "!=", ">", "<", ">=", "<=")
                )
                .Transform(op => new Token(op, TokenType.Operator));

            var strings = new StringParser();

            var allTokens = First(
                If(End(), Produce(() => Token.EndOfInput())),
                cSharpLiterals,
                words,
                operators,
                numbers,
                strings,
                chars,
                Produce((t, d) => new Token(t.GetNext().ToString(), TokenType.Unknown))
            );

            var whitespace = Match(char.IsWhiteSpace).ListCharToString(true);
            var whitespaceOrComment = First(
                whitespace,
                singleLineComments,
                multiLineComments
            );

            return Rule(
                // TODO: Get a list of all whitespace and comments, and include those in the Token we return
                whitespaceOrComment.List(),
                Produce((t, d) => t.CurrentLocation),
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
            var hexDigits = Match(c => _hexDigits.Contains(c));

            var hexCharLiteral = Rule(
                Match(c => c == '\\'),
                Match(c => c == 'x'),
                // TODO: 1-4 of these only
                hexDigits.ListCharToString(),

                (slash, x, c) => "\\x" + c
            );

            var unicodeCharLiteral = Rule(
                Match(c => c == '\\'),
                Match(c => c == 'u'),
                // TODO: 1-4 of these or exactly-4 of these?
                hexDigits.ListCharToString(),

                (slash, u, c) => "\\u" + c
            );

            var charEscape = Rule(
                Match(c => c == '\\'),
                Match(c => "abfnrtv\\'\"0".Contains(c)),

                (slash, c) => "\\" + c
            );

            var chars = Rule(
                Match(c => c == '\''),
                First(
                    hexCharLiteral,
                    unicodeCharLiteral,
                    charEscape,
                    Transform(Match(c => c != '\0' && c != '\''), c => c.ToString()),
                    Produce(() => (string)null)
                ),
                First(
                    Match(c => c == '\''),
                    Produce(() => '\0')
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
            var hexDigits = Match(c => _hexDigits.Contains(c));
            var digits = Match(char.IsDigit);

            // "0x" <hexDigit> (("_" <hexDigit>) | <hexDigit>)*
            var hexNumber = Rule(
                Match("0x"),
                hexDigits,
                First(
                        Rule(
                            Match(c => c == '_'),
                            hexDigits,

                            (sep, digit) => new[] { sep, digit }
                        ),
                        digits.Transform(c => new[] { c })
                    )
                    .List().Transform(x => new string(x.SelectMany(y => y).ToArray())),
                First(
                    Match("UL").Transform(c => TokenType.ULong),
                    Match("U").Transform(c => TokenType.UInteger),
                    Match("L").Transform(c => TokenType.Long),
                    Produce(() => TokenType.Integer)
                ),

                (prefix, first, rest, type) => new Token(int.Parse(first + rest, System.Globalization.NumberStyles.HexNumber).ToString(), type)
            );

            // <digit> (("_" <digit>) | <digit>)*
            var digitList = Rule(
                digits,
                First(
                        Rule(
                            Match(c => c == '_'),
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
                Optional(Match("-").Transform(c => "-"), () => ""),
                digitList,
                Match(".").Transform(c => "."),
                digitList,
                First(
                    Match("F").Transform(c => TokenType.Float),
                    Match("M").Transform(c => TokenType.Decimal),
                    Produce(() => TokenType.Double)
                ),

                (neg, whole, dot, fract, type) => new Token(neg + whole + "." + fract, type)
            );

            // "-"? <digitList>
            var integralNumber = Rule(
                Optional(Match("-").Transform(c => "-"), () => ""),
                digitList,
                First(
                    Match("UL").Transform(c => TokenType.ULong),
                    Match("U").Transform(c => TokenType.UInteger),
                    Match("L").Transform(c => TokenType.Long),
                    Produce(() => TokenType.Integer)
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
