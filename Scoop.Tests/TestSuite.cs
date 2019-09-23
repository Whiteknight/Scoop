using System;
using System.IO;
using NUnit.Framework;
using Scoop.Grammar;

namespace Scoop.Tests
{
    [SetUpFixture]
    public class TestSuite
    {
        private static string _testRunId;
        //private static DirectoryInfo _testDirectory;
        private static ScoopL1Grammar _scoopGrammar;

        public static string GetTestRunId() => _testRunId;
        //public static string GetTestDirectoryPath() => _testDirectory.FullName;
        public static ScoopL1Grammar GetScoopGrammar() => _scoopGrammar ?? (_scoopGrammar = new ScoopL1Grammar());

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            _testRunId = Guid.NewGuid().ToString("N");
            //Directory.CreateDirectory(_testRunId);
            _scoopGrammar = new ScoopL1Grammar();
        }

        [OneTimeTearDown]
        public void GlobalCleanup()
        {
            //_testDirectory.Delete(true);
        }
    }
}
