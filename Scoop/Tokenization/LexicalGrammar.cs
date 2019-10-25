using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers;
using static Scoop.Parsers.ParserMethods;
using static Scoop.Tokenization.TokenParserMethods;

namespace Scoop.Tokenization
{
    public static class LexicalGrammar
    {
        public static IParser<char, Token> GetParser()
        {
            // TODO: "_" separator in a number
            // "0x" <hexDigit>+ | "-"? <digit>+ "." <digit>+ <type>? | "-"? <digit>+ <type>?
            var _hexDigits = new HashSet<char>("abcdefABCDEF0123456789");
            var numbers = First(
                Sequence(
                    Match("0x", c => c),
                    List(
                        Match<char>(c => _hexDigits.Contains(c)),
                        c => new string(c.ToArray()),
                        atLeastOne: true
                    ),
                    First(
                        Match("UL", c => TokenType.ULong),
                        Match("U", c => TokenType.UInteger),
                        Match("L", c => TokenType.Long),
                        Produce<char, TokenType>(() => TokenType.Integer)
                    ),
                    (prefix, body, type) => new Token(int.Parse(body, System.Globalization.NumberStyles.HexNumber).ToString(), type)
                ),
                Sequence(
                    Optional(Match("-", c => "-"), () => ""),
                    List(
                        Match<char>(char.IsDigit),
                        c => new string(c.ToArray()),
                        atLeastOne: true
                    ),
                    Match(".", c => "."),
                    List(
                        Match<char>(char.IsDigit),
                        c => new string(c.ToArray()),
                        atLeastOne: true
                    ),
                    First(
                        Match("F", c => TokenType.Float),
                        Match("M", c => TokenType.Decimal),
                        Produce<char, TokenType>(() => TokenType.Double)
                    ),
                    (neg, whole, dot, fract, type) => new Token(neg + whole + "." + fract, type)
                ),
                Sequence(
                    Optional(Match("-", c => "-"), () => ""),
                    List(
                        Match<char>(char.IsDigit),
                        c => new string(c.ToArray()),
                        atLeastOne: true
                    ),
                    First(
                        Match("UL", c => TokenType.ULong),
                        Match("U", c => TokenType.UInteger),
                        Match("L", c => TokenType.Long),
                        Produce<char, TokenType>(() => TokenType.Integer)
                    ),
                    (neg, whole, type) => new Token(neg + whole, type)
                )
            );

            var words = Sequence(
                First(
                    Match("@", c => "@"),
                    Produce<char, string>(() => "")
                ),
                Match<char>(c => char.IsLetter(c) || c == '_'),
                List(
                    Match<char>(c => char.IsLetter(c) || char.IsDigit(c) || c == '_'),
                    c => c.ToArray()
                ),
                (prefix, start, rest) => new Token(prefix + start + new string(rest), TokenType.Word)
            );

            var chars = Sequence(
                Match<char>(c => c == '\''),
                First(
                    Sequence(
                        Match<char>(c => c == '\\'),
                        Match<char>(c => c == 'x'),
                        List(
                            // TODO: 1-4 of these only
                            Match<char>(c => _hexDigits.Contains(c)),
                            t => new string(t.ToArray())
                        ),
                        (slash, x, c) => "\\x" + c
                    ),
                    Sequence(
                        Match<char>(c => c == '\\'),
                        Match<char>(c => c == 'u'),
                        List(
                            // TODO: 1-4 of these or exactly-4 of these?
                            Match<char>(c => _hexDigits.Contains(c)),
                            t => new string(t.ToArray())
                        ),
                        (slash, u, c) => "\\u" + c
                    ),
                    Sequence(
                        Match<char>(c => c == '\\'),
                        Match<char>(c => "abfnrtv\\'\"0".Contains(c)),
                        (slash, c) => "\\" + c
                    ),
                    Transform(Match<char>(c => c != '\0'), c => c.ToString())
                    // TODO: How to handle this case?
                    //Error<char, char>(t => $"Expected char value but found {t.Peek()}")
                ),
                First(
                    Match<char>(c => c == '\''),
                    // TODO: Need to communicate an error here, but for now we'll create a synthetic char to fill the gap
                    Produce<char, char>(() => '\'')
                    //Error<char, char>(t => $"Expected end singlequote but found {t.Peek()}")
                ),
                (start, content, end) => new Token("'" + content + "'", TokenType.Character)
            );

            var allTokens = First(
                Match("\0", c => Token.EndOfInput()),
                Sequence(
                    Match("//", c => new string(c)),
                    List(Match<char>(c => c != '\r' && c != '\n'), l => new string(l.ToArray())),
                    (prefix, content) => new Token(prefix + content, TokenType.Comment)
                ).Named("C++ Comment"),
                new MultilineCommentParser(),
                new CSharpLiteralParser(),
                words,
                new OperatorParser(),
                numbers,
                new StringParser(),
                chars,
                Produce<char, Token>(t => new Token(t.GetNext().ToString(), TokenType.Unknown))
            );

            return Sequence(
                List(Match<char>(char.IsWhiteSpace), t => (object) null),
                Produce<char, Location>(t => t.CurrentLocation),
                allTokens,

                (ws, location, token) =>
                {
                    token.Location = location;
                    // TODO: Should we keep track of the leading whitespace at all?
                    return token;
                }
            );
        }
    }
}