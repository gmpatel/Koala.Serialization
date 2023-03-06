using NUnit.Framework;
using Newtonsoft.Json;
using UnitTests.TestModels;

namespace UnitTests
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
            var data1 = new Data 
            { 
                Prop1 = "test1", 
                Prop2 = "test2" 
            };
            
            var data2 = data1.JsonClone();

            var encrypted = data1.Encrypt();
            var decrypted = encrypted.Decrypt<Data>();

            var compressedPlus = data1.Compress();
            var uncompressedPlus = compressedPlus.Uncompress<Data>();

            var compressed = data1.Compress();
            var uncompressed = compressed.Uncompress<Data>();

            var data = 13423245353;

            var enc = data.Encrypt();
            var dec = enc.Decrypt<long>();


            Assert.AreNotSame(data1, data2);
            Assert.AreEqual(data1.Prop1, data2.Prop1);
            Assert.AreEqual(data1.Prop2, data2.Prop2);
        }
    }
}