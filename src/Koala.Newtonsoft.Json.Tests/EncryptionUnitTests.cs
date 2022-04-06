using NUnit.Framework;
using Newtonsoft.Json;

namespace Koala.Newtonsoft.Json.Tests
{
    public class EncryptionUnitTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var textData = "I have sample data which is secret";

            var encryptedData = textData.EncryptToString();

            var originalData = encryptedData.Decrypt<string>();
        }
    }
}