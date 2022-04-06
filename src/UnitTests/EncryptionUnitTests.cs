using NUnit.Framework;
using Newtonsoft.Json;

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
            var person1 = new Data { Id = 1, Name = "Dhyey Patel" };
            
            var personCompress = person1.Compress();

            var person2 = personCompress.Uncompress<Data>();


            var data = "This is sample data";

            var dataCompress = data.Compress();

            var data2 = dataCompress.Uncompress<string>();
        }
    }

    public class Data
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}