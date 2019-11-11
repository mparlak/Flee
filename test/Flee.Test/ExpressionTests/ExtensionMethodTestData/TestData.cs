namespace Flee.OtherTests.ExtensionMethodTestData
{
    using Microsoft.Win32.SafeHandles;

    internal class TestData
    {
        public string Id { get; set; }

        public SubTestData Sub
        {
            get { return new SubTestData { Id = "Sub" + this.Id }; }
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

    internal class SubTestData
    {
        public string Id { get; set; }

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
