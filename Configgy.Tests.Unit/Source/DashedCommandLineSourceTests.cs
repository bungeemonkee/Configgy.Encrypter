﻿using Configgy.Source;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Configgy.Tests.Unit.Source
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DashedCommandLineSourceTests
    {
        [TestMethod]
        public void Get_Incudes_Defined_Values()
        {
            const string name = "Testing";
            const string expected = "Blah";
            var commandLine = new [] { "--Testing=Blah" };

            var propertyMock = new Mock<PropertyInfo>();
            propertyMock.Setup(x => x.GetCustomAttributes(true)).Returns(new object[] { });

            var source = new DashedCommandLineSource(commandLine);

            string value;
            var result = source.Get(name, propertyMock.Object, out value);

            Assert.AreEqual(expected, value);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Get_Assumes_True_For_Booean_With_No_Value()
        {
            const string name = "Testing";
            const string expected = "True";
            var commandLine = new [] { "--Testing" };

            var propertyMock = new Mock<PropertyInfo>();
            propertyMock.SetupGet(x => x.PropertyType).Returns(typeof(bool));

            var source = new DashedCommandLineSource(commandLine);

            string value;
            var result = source.Get(name, propertyMock.Object, out value);

            propertyMock.VerifyGet(x => x.PropertyType);
            Assert.AreEqual(expected, value);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Get_Returns_Null_For_Non_Booean_With_No_Value()
        {
            const string name = "Testing";
            var commandLine = new [] { "--Testing" };

            var propertyMock = new Mock<PropertyInfo>();
            propertyMock.SetupGet(x => x.PropertyType).Returns(typeof(string));

            var source = new DashedCommandLineSource(commandLine);

            string value;
            var result = source.Get(name, propertyMock.Object, out value);

            propertyMock.VerifyGet(x => x.PropertyType);
            Assert.IsNull(value);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Get_Ignores_Unknown_Options()
        {
            const string name = "Testing";
            var commandLine = new [] { "--Banana=Blah" };

            var propertyMock = new Mock<PropertyInfo>();
            propertyMock.Setup(x => x.GetCustomAttributes(true)).Returns(new object[] { });

            var source = new DashedCommandLineSource(commandLine);

            string value;
            var result = source.Get(name, propertyMock.Object, out value);

            Assert.IsNull(value);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Get_Is_Not_Case_Sensitive()
        {
            const string name = "Testing";
            const string expected = "Blah";
            var commandLine = new [] { "--tEstinG=Blah" };

            var propertyMock = new Mock<PropertyInfo>();
            propertyMock.Setup(x => x.GetCustomAttributes(true)).Returns(new object[] { });

            var source = new DashedCommandLineSource(commandLine);

            string value;
            var result = source.Get(name, propertyMock.Object, out value);

            Assert.AreEqual(expected, value);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Get_Uses_Name_Override_From_CommandLineNameAttribute()
        {
            const string name = "Testing";
            const string nameOverride = "test";
            const string expected = "1234";
            var commandLine = new[] { "--test=1234" };

            var attribute = new CommandLineNameAttribute(nameOverride);

            var propertyMock = new Mock<PropertyInfo>();
            propertyMock.Setup(x => x.GetCustomAttributes(true)).Returns(new object[] { attribute });

            var source = new DashedCommandLineSource(commandLine);

            string value;
            var result = source.Get(name, propertyMock.Object, out value);
            
            propertyMock.VerifyAll();
            Assert.IsTrue(result);
            Assert.AreEqual(expected, value);
        }
    }
}
