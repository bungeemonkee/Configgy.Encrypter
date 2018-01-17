﻿
using System.Diagnostics.CodeAnalysis;

namespace Configgy.Tests.Unit.Coercion
{
    [ExcludeFromCodeCoverage]
    public class ClassWithNoDefaultConstructor
    {
        public readonly int Value;

        public ClassWithNoDefaultConstructor(int value)
        {
            Value = value;
        }
    }
}
