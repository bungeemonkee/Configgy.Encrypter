﻿using Configgy.Coercion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Configgy.Tests.Unit.Coercion
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class JsonCoercerTests
    {
        [TestMethod]
        public void JsonCoercer_Coerce_Works_With_Array_Of_Int()
        {
            const string input = "[1,4,78,222]";
            var expected = new [] { 1, 4, 78, 222 };

            var coercer = new JsonCoercerAttribute();

            int[] result;
            var coerced = coercer.Coerce(input, null, null, out result);

            CollectionAssert.AreEqual(expected, result);
            Assert.IsTrue(coerced);
        }

        [TestMethod]
        public void JsonCoercer_Coerce_Works_With_Dictionary_Of_String_String()
        {
            const string input = "{\"Banana\":\"Good\",\"Apple\":\"Yummy\",\"Radish\":\"Icky\"}";
            var expected = new Dictionary<string, string>
            {
                ["Banana"] = "Good",
                ["Apple"] = "Yummy",
                ["Radish"] = "Icky"
            };

            var coercer = new JsonCoercerAttribute();

            Dictionary<string, string> result;
            var coerced = coercer.Coerce(input, null, null, out result);

            CollectionAssert.AreEqual(expected, result);
            Assert.IsTrue(coerced);
        }

        [TestMethod]
        [ExpectedException(typeof(SerializationException))]
        public void JsonCoercer_Coerce_Throws_Exception_With_Invalid_Json()
        {
            const string input = "{";

            var coercer = new JsonCoercerAttribute();

            Dictionary<string, string> result;
            var coerced = coercer.Coerce(input, null, null, out result);

            Assert.IsNull(result);
            Assert.IsFalse(coerced);
        }

        [TestMethod]
        public void JsonCoercer_Coerce_Returns_Null_With_Null_Json()
        {
            const string input = null;

            var coercer = new JsonCoercerAttribute();

            Dictionary<string, string> result;
            var coerced = coercer.Coerce(input, null, null, out result);

            Assert.IsNull(result);
            Assert.IsTrue(coerced);
        }
    }
}
