namespace Flee.ExtensionMethodTests.ExtensionMethodTestData
{
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

        /// <summary>
        /// A bug previous meant a small difference in
        /// parameters was not detected and treated
        /// as ambiguous
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public string MatchParams(uint x, int y, int z)
        {
            return "UII";
        }
        public string MatchParams(int x, int y, int z)
        {
            return "III";
        }
        public string MatchParams(float x, float y, double z)
        {
            return "FFD";
        }
        public string MatchParams(double x, double y, double z)
        {
            return "DDD";
        }
    }
}
