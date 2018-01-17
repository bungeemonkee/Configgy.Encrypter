﻿using Configgy.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Configgy.Tests.Unit.Validation
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RegexValidatorTests
    {
        [TestMethod]
        public void Validate_Allows_Valid_Value()
        {
            var valdator = new RegexValidatorAttribute("aaabbbccc");

            string value;
            valdator.Validate("aaabbbccc", "testing", null, out value);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void Validate_Throws_Exception_For_Invalid_Value()
        {
            var valdator = new RegexValidatorAttribute("aaabbbccc");

            string value;
            valdator.Validate("the number twelve", "testing", null, out value);
        }
    }
}
