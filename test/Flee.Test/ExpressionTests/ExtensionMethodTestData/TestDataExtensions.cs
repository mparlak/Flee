namespace Flee.OtherTests.ExtensionMethodTestData
{
    internal static class TestDataExtensions
    {
        public static string SayHello(this object data)
        {
            if (data is TestData td)
            {
                return "Hello " + td.Id;
            }

            return "Hello unkown!";
        }

        public static string SayHello(this TestData data)
        {
            return "Hello " + data.Id;
        }

        public static string SayHello(this TestData data, string suffix)
        {
            return "Hello " + data.Id + suffix;
        }

        public static string SayHello(this TestData data, bool friendly)
        {
            return "Hello " + (friendly ? "dear " : string.Empty) + data.Id;
        }

        public static string SayHello(this SubTestData data)
        {
            return "Hello as well, " + data.Id;
        }

        public static string SayHello(this SubTestData data, string suffix)
        {
            return "Hello as well, " + data.Id + suffix;
        }

        public static string SayHello(this SubTestData data, bool friendly)
        {
            return "Hello as well, " + (friendly ? "dear " : string.Empty) + data.Id;
        }
    }
}
