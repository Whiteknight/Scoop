using System;
using FluentAssertions;
using NUnit.Framework;
using Scoop.Parsing.Sequences;
using Scoop.Parsing.Tokenization;

namespace Scoop.Tests.Tokenizing
{
    [TestFixture]
    public class StringCharacterSequenceTests
    {
        [Test]
        public void GetNext_Empty()
        {
            var target = new StringCharacterSequence("");
            // Every get attempt past the end of the string will return '\0'
            target.GetNext().Should().Be('\0');
            target.GetNext().Should().Be('\0');
            target.GetNext().Should().Be('\0');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_Chars()
        {
            var target = new StringCharacterSequence("abc");
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void PutBack_Test()
        {
            var target = new StringCharacterSequence("abc");
            target.GetNext().Should().Be('a');
            target.PutBack('a');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.PutBack('b');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void Peek_Test()
        {
            var target = new StringCharacterSequence("abc");
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }
    }
}
