using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Scoop.Parsing.Sequences;

namespace Scoop.Tests.Tokenizing
{
    [TestFixture]
    public class StreamCharacterSequenceTests
    {
        [Test]
        public void GetNext_Test()
        {
            var sc = "abc";
            var memoryStream = new MemoryStream();
            memoryStream.Write(Encoding.UTF8.GetBytes(sc));
            memoryStream.Seek(0, SeekOrigin.Begin);

            var target = new StreamCharacterSequence(memoryStream, Encoding.UTF8);
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');

            memoryStream.Dispose();
            target.Dispose();
        }

        [Test]
        public void PutBack_Test()
        {
            var sc = "abc";
            var memoryStream = new MemoryStream();
            memoryStream.Write(Encoding.UTF8.GetBytes(sc));
            memoryStream.Seek(0, SeekOrigin.Begin);

            var target = new StreamCharacterSequence(memoryStream, Encoding.UTF8);
            target.GetNext().Should().Be('a');
            target.PutBack('a');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.PutBack('b');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }
    }
}