using System;
using System.IO;
using NUnit.Framework;
using Scoop.Parsing;

namespace Scoop.Tests
{
    [SetUpFixture]
    public class TestSuite
    {
        private static string _testRunId;
        //private static DirectoryInfo _testDirectory;
        private static ScoopGrammar _scoopGrammar;

        public static string GetTestRunId() => _testRunId;
        //public static string GetTestDirectoryPath() => _testDirectory.FullName;
        public static ScoopGrammar GetGrammar() => _scoopGrammar ?? (_scoopGrammar = new ScoopGrammar());

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            _testRunId = Guid.NewGuid().ToString("N");
            //Directory.CreateDirectory(_testRunId);
            //_scoopGrammar = new ScoopGrammar();
        }

        [OneTimeTearDown]
        public void GlobalCleanup()
        {
            //_testDirectory.Delete(true);
        }
    }
}
