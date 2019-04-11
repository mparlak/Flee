namespace Flee.Test.ExpressionTests.ExtensionMethodTestData
{
    internal class TestData
    {
        public string Id { get; set; }

        public TestData Sub => new TestData { Id = "Sub" + this.Id };

        public string SayHello(int times)
        {
            string result = string.Empty;
            for (int i = 0; i < times; i++)
            {
                result += "hello ";
            }

            return result + Id;
        }
    }
}
