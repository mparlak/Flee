namespace Flee.ExtensionMethodTests.ExtensionMethodTestData
{
    internal static class TestDataExtensions
    {
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
    }
}
