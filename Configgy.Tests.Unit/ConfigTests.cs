﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Configgy.Cache;
using Moq;
using Configgy.Tests.Unit.Cache;
using Configgy.Source;
using System.Reflection;
using Configgy.Validation;
using Configgy.Coercion;
using Configgy.Exceptions;

namespace Configgy.Tests.Unit
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void ClearCache_Calls_Cache_Clear()
        {
            var cacheMock = new Mock<IValueCache>();

            var config = new ConfigWrapper<object>(cacheMock.Object, null, null, null);

            config.ClearCache();

            cacheMock.Verify(c => c.Clear(), Times.Once);
        }

        [TestMethod]
        public void Get_Calls_Cache_GetValue()
        {
            const string name = "__value__";

            var expected = new object();

            var cacheMock = new Mock<IValueCache>();
            cacheMock.Setup(c => c.GetValue(name, It.IsAny<Func<string, object>>()))
                .Returns(expected);

            var config = new ConfigWrapper<object>(cacheMock.Object, null, null, null);

            var result = config.Get_Wrapper(name);

            cacheMock.Verify(c => c.GetValue(name, It.IsAny<Func<string, object>>()), Times.Once);
            Assert.AreSame(expected, result);
        }

        [TestMethod]
        public void Get_Calls_Source_GetRawValue_Validator_Validate_And_Coercer_Coerce_When_Callback_Invoked()
        {
            const string name = "__value__";

            var expectedRaw = "1";
            var expectedValue = 1;
            var cache = new TestingCache();

            var sourceMock = new Mock<IValueSource>();
            sourceMock.Setup(s => s.GetRawValue(name, null))
                .Returns(expectedRaw);

            var validatorMock = new Mock<IValueValidator>();
            validatorMock.Setup(s => s.Validate<int>(expectedRaw, name, null));

            var coercerMock = new Mock<IValueCoercer>();
            coercerMock.Setup(c => c.CoerceTo<int>(expectedRaw, name, null))
                .Returns(expectedValue);

            var config = new ConfigWrapper<int>(cache, sourceMock.Object, validatorMock.Object, coercerMock.Object);

            var result = config.Get_Wrapper(name);

            sourceMock.Verify(s => s.GetRawValue(name, null), Times.Once);
            validatorMock.Verify(v => v.Validate<int>(expectedRaw, name, null), Times.Once);
            coercerMock.Verify(c => c.CoerceTo<int>(expectedRaw, name, null), Times.Once);
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void Get_Calls_Source_GetRawValue_Validator_Validate_And_Coercer_Coerce_When_Callback_Invoked_Via_Property()
        {
            const string name = ConfigWrapper<int>.ThePropertyName;

            var expectedRaw = "1";
            var expectedValue = 1;
            var cache = new TestingCache();

            var sourceMock = new Mock<IValueSource>();
            sourceMock.Setup(s => s.GetRawValue(name, It.IsAny<PropertyInfo>()))
                .Returns(expectedRaw);

            var validatorMock = new Mock<IValueValidator>();
            validatorMock.Setup(s => s.Validate<int>(expectedRaw, name, It.IsAny<PropertyInfo>()));

            var coercerMock = new Mock<IValueCoercer>();
            coercerMock.Setup(c => c.CoerceTo<int>(expectedRaw, name, It.IsAny<PropertyInfo>()))
                .Returns(expectedValue);

            var config = new ConfigWrapper<int>(cache, sourceMock.Object, validatorMock.Object, coercerMock.Object);

            var result = config.TheProperty;

            sourceMock.Verify(s => s.GetRawValue(name, It.IsAny<PropertyInfo>()), Times.Once);
            validatorMock.Verify(v => v.Validate<int>(expectedRaw, name, It.IsAny<PropertyInfo>()), Times.Once);
            coercerMock.Verify(c => c.CoerceTo<int>(expectedRaw, name, It.IsAny<PropertyInfo>()), Times.Once);
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        [ExpectedException(typeof(MissingValueException))]
        public void Get_Throws_MissingValueException_When_Source_GetRawValue_Returns_Null()
        {
            const string name = "__value__";
            
            var cache = new TestingCache();

            var sourceMock = new Mock<IValueSource>();
            sourceMock.Setup(s => s.GetRawValue(name, null))
                .Returns((string)null);

            var config = new ConfigWrapper<int>(cache, sourceMock.Object, null, null);

            var result = config.Get_Wrapper(name);
        }

        [TestMethod]
        [ExpectedException(typeof(CoercionException))]
        public void Get_Throws_CoercionException_When_Coercer_Coerce_Returns_Null()
        {
            const string name = "__value__";

            var expectedRaw = "1";
            var cache = new TestingCache();

            var sourceMock = new Mock<IValueSource>();
            sourceMock.Setup(s => s.GetRawValue(name, It.IsAny<PropertyInfo>()))
                .Returns(expectedRaw);

            var validatorMock = new Mock<IValueValidator>();
            validatorMock.Setup(s => s.Validate<int>(expectedRaw, name, It.IsAny<PropertyInfo>()));

            var coercerMock = new Mock<IValueCoercer>();
            coercerMock.Setup(c => c.CoerceTo<int>(expectedRaw, name, It.IsAny<PropertyInfo>()))
                .Returns((object)null);

            var config = new ConfigWrapper<int>(cache, sourceMock.Object, validatorMock.Object, coercerMock.Object);

            var result = config.Get_Wrapper(name);
        }
    }
}