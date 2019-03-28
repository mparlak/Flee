namespace Flee.ExtensionMethodTests.ExtensionMethodTestData
{
    using Microsoft.Win32.SafeHandles;

    internal class TestData
    {
        public string Id { get; set; }

        public TestData Sub
        {
            get { return new TestData { Id = "Sub" + this.Id }; }
        }

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
