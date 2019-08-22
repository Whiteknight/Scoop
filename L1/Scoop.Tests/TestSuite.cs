using System;
using System.IO;
using NUnit.Framework;

namespace Scoop.Tests
{
    [SetUpFixture]
    public class TestSuite
    {
        private static string _testRunId;
        private static DirectoryInfo _testDirectory;

        public static string GetTestRunId() => _testRunId;
        public static string GetTestDirectoryPath() => _testDirectory.FullName;

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            _testRunId = Guid.NewGuid().ToString("N");
            //Directory.CreateDirectory(_testRunId);
        }

        [OneTimeTearDown]
        public void GlobalCleanup()
        {
            //_testDirectory.Delete(true);
        }
    }
}
