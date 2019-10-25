using FluentAssertions;
using FluentAssertions.Primitives;
using Scoop.Parsing.Tokenization;

namespace Scoop.Tests.Utility
{
    public class TokenAssertions : ReferenceTypeAssertions<Token, TokenAssertions>
    {
        protected override string Identifier => "Token";

        public TokenAssertions(Token subject) : base(subject)
        {
        }

        public AndConstraint<TokenAssertions> Match(TokenType type, string value)
        {
            Subject.Type.Should().Be(type);
            Subject.Value.Should().Be(value);
            return new AndConstraint<TokenAssertions>(this);
        }
    }
}