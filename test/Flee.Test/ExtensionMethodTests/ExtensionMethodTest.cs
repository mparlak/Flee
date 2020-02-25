namespace Flee.ExtensionMethodTests
{
    using Flee.ExtensionMethodTests.ExtensionMethodTestData;
    using global::Flee.PublicTypes;
    using global::Flee.Test.Infrastructure;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>The extension method test.</summary>
    [TestClass]
    public class ExtensionMethodTest : ExpressionTests
    {
        [TestMethod]
        public void TestExtensionMethodCallOnOwner()
        {
            var result = GetExpressionContext().CompileDynamic("SayHello()").Evaluate();
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnProperty()
        {
            var result = GetExpressionContext().CompileDynamic("Sub.SayHello()").Evaluate();
            Assert.AreEqual("Hello SubWorld", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnOwnerWithArguments()
        {
            var result = GetExpressionContext().CompileDynamic("SayHello(\"!!!\")").Evaluate();
            Assert.AreEqual("Hello World!!!", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnOwnerWithArgumentsOnOverload()
        {
            var result = GetExpressionContext().CompileDynamic("SayHello(true)").Evaluate();
            Assert.AreEqual("Hello dear World", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnOwnerWithArgumentsOnClassOverload()
        {
            var result = GetExpressionContext().CompileDynamic("SayHello(2)").Evaluate();
            Assert.AreEqual("hello hello World", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnPropertyWithArguments()
        {
            var result = GetExpressionContext().CompileDynamic("Sub.SayHello(\"!!!\")").Evaluate();
            Assert.AreEqual("Hello SubWorld!!!", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnPropertyWithArgumentsOnClassOverload()
        {
            var result = GetExpressionContext().CompileDynamic("Sub.SayHello(2)").Evaluate();
            Assert.AreEqual("hello hello SubWorld", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnPropertyWithArgumentsOnOverload()
        {
            var result = GetExpressionContext().CompileDynamic("Sub.SayHello(\"!!!\")").Evaluate();
            Assert.AreEqual("Hello SubWorld!!!", result);
        }

        private static ExpressionContext GetExpressionContext()
        {
            var expressionOwner = new TestData { Id = "World" };
            var context = new ExpressionContext(expressionOwner);
            context.Imports.AddType(typeof(TestDataExtensions));
            return context;
        }
    }
}
