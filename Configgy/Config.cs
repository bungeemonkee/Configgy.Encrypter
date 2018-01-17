﻿using Configgy.Cache;
using Configgy.Source;
using Configgy.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Configgy.Validation;
using Configgy.Coercion;
using System;
using Configgy.Transformation;

namespace Configgy
{
    /// <summary>
    /// The main Configgy configuration base class.
    /// </summary>
    public abstract class Config
    {
        private static readonly Type StringType = typeof(string);

        private readonly IValueCache _cache;
        private readonly IValueSource _source;
        private readonly IValueTransformer _transformer;
        private readonly IValueValidator _validator;
        private readonly IValueCoercer _coercer;
        private readonly IReadOnlyDictionary<string, PropertyInfo> _properties;

        /// <summary>
        /// Create a default Config instance.
        /// </summary>
        protected Config()
            : this(new DictionaryCache(), new AggregateSource(), new AggregateTransformer(), new AggregateValidator(), new AggregateCoercer())
        {
        }

        /// <summary>
        /// Create a default Config instance and include parameters from a command line.
        /// </summary>
        /// <param name="commandLine">The command line to parse configuration values from.</param>
        protected Config(string[] commandLine)
            : this(new DictionaryCache(), new AggregateSource(commandLine), new AggregateTransformer(), new AggregateValidator(), new AggregateCoercer())
        {
        }

        /// <summary>
        /// Create a totally customized Config instance.
        /// Generally you will not need this constructor.
        /// </summary>
        /// <param name="cache">The <see cref="IValueCache"/> instance to be used by this Config instance.</param>
        /// <param name="source">The <see cref="IValueSource"/> instance to be used by this Config instance.</param>
        /// <param name="transformer">The <see cref="IValueTransformer"/> instance to be used by this Config instance.</param>
        /// <param name="validator">The <see cref="IValueValidator"/> instance to be used by this Config instance.</param>
        /// <param name="coercer">The <see cref="IValueCoercer"/> instance to be used by this Config instance.</param>
        protected Config(IValueCache cache, IValueSource source, IValueTransformer transformer, IValueValidator validator, IValueCoercer coercer)
        {
            _cache = cache;
            _source = source;
            _transformer = transformer;
            _validator = validator;
            _coercer = coercer;

            // Pre-cache all the properties on this instance
            _properties = GetType()
                .GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .OfType<PropertyInfo>()
                .Where(p => p.CanRead)
                .ToDictionary(p => p.Name);
        }

        /// <summary>
        /// Clear all cached configuration values.
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Get a configuration value.
        /// </summary>
        /// <typeparam name="T">The type of the expected configuration value.</typeparam>
        /// <param name="valueName">
        ///     The name of the configuration value to get.
        ///     This will automatically be the name of the calling method or property and will be populated by the compiler.
        /// </param>
        /// <returns>The configuration value.</returns>
        protected T Get<T>([CallerMemberName] string valueName = null)
        {
            return (T)_cache.Get(valueName, ProduceValue<T>);
        }

        private object ProduceValue<T>(string valueName)
        {
            // get the property reference
            PropertyInfo property;
            _properties.TryGetValue(valueName, out property);

            // Get the value from the factory
            string value;
            if (!_source.Get(valueName, property, out value))
            {
                // Throw an exception informing the user of the missing value
                throw new MissingValueException(valueName);
            }

            // Transform the value
            value = _transformer.Transform(value, valueName, property);

            // Validate the value
            T result;
            var coerced = _validator.Validate(value, valueName, property, out result);

            // Optimization: skip coercion for string values
            var type = typeof(T);
            if (type == StringType) return value;

            // Optimization: if the validator did the coercion the just return that value
            if (coerced) return result;

            // Coerce the value
            if (!_coercer.Coerce(value, valueName, property, out result))
            {
                // Throw an exception informing the user of the failed coercion
                throw new CoercionException(value, valueName, type, property);
            }

            // Return the result
            return result;
        }
    }
}
