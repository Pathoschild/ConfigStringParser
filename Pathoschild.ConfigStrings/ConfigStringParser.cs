using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Pathoschild.ConfigStrings
{
    /// <summary>Parses strings representing arbitrary configuration settings using the connection string idiom.</summary>
    public class ConfigStringParser : IEnumerable<KeyValuePair<string, object>>
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying connection string builder used to parse the connection string.</summary>
        private readonly DbConnectionStringBuilder Builder;


        /*********
        ** Accessors
        *********/
        /// <summary>The number of keys contained by the config string.</summary>
        public int Count => this.Builder.Count;

        // ReSharper disable once AssignNullToNotNullAttribute
        /// <summary>The value keys in the config string.</summary>
        public IEnumerable<string> Keys => (from string key in this.Builder.Keys select key);

        /// <summary>The parsed config string.</summary>
        public string ConfigString => this.Builder.ConnectionString;

        /// <summary>Get or set a value by its key.</summary>
        /// <param name="key">The entry key.</param>
        public object this[string key]
        {
            get { return this.Builder.ContainsKey(key) ? this.Builder[key] : null; }
            set { this.Builder[key] = value; }
        }


        /*********
        ** Public methods
        *********/
        /***
        ** Constructors
        ***/
        /// <summary>Construct an instance.</summary>
        public ConfigStringParser()
        {
            this.Builder = new DbConnectionStringBuilder();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="configString">The config string which contains serialised key-value settings.</param>
        public ConfigStringParser(string configString)
        {
            this.Builder = new DbConnectionStringBuilder { ConnectionString = configString };
        }

        /***
        ** Read methods
        ***/
        /// <summary>Get whether the config string contains a specific key.</summary>
        /// <param name="key">The key to locate.</param>
        public bool ContainsKey(string key) => this.Builder.ContainsKey(key);

        /// <summary>Get a value from the config string.</summary>
        /// <param name="key">The entry key.</param>
        /// <param name="required">Whether to throw an exception if the value is empty or not defined.</param>
        /// <exception cref="KeyNotFoundException">The key was not found and <paramref name="required"/> is <c>true</c>.</exception>
        /// <exception cref="FormatException">The value can't be converted to the requested <typeparamref name="TValue"/>.</exception>
        public object Get(string key, bool required = true)
        {
            // validate missing field
            if (!this.ContainsKey(key))
            {
                if (required)
                    throw new KeyNotFoundException($"The config string doesn't contain a value for the '{key}' key. Valid keys: '{String.Join(", ", this.Keys)}'.");
                return null;
            }
            return this.Builder[key];
        }

        /// <summary>Get a value from the config string.</summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="key">The entry key.</param>
        /// <param name="required">Whether to throw an exception if the value is empty or not defined.</param>
        /// <exception cref="KeyNotFoundException">The key was not found and <paramref name="required"/> is <c>true</c>.</exception>
        /// <exception cref="FormatException">The value can't be converted to the requested <typeparamref name="TValue"/>.</exception>
        public TValue Get<TValue>(string key, bool required = true)
        {
            return (TValue)this.Get(key, typeof(TValue), required);
        }

        /// <summary>Assert that the specified fields have all been defined.</summary>
        /// <param name="keys">The required field keys.</param>
        /// <exception cref="KeyNotFoundException">Some required keys were not defined.</exception>
        public void AssertRequiredKeys(IEnumerable<string> keys)
        {
            string[] missingFields = keys.Where(k => !this.ContainsKey(k)).ToArray();
            if (missingFields.Any())
                throw this.GetMissingKeysException(missingFields);
        }

        /// <summary>Map the parsed settings to a model.</summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <returns>Returns the mapped model.</returns>
        public TModel MapTo<TModel>() where TModel : class, new() => this.MapTo(new TModel());

        /// <summary>Map the parsed settings to a model.</summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <exception cref="FormatException">Some required keys were not defined.</exception>
        public TModel MapTo<TModel>(TModel model)
        {
            // analyse model
            var properties = (
                from property in typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where property.CanWrite
                let displayName = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? property.Name
                select new
                {
                    Name = property.Name,
                    DisplayName = displayName,
                    Required = property.GetCustomAttribute<RequiredAttribute>() != null,
                    Property = property,
                    HasValue = this.ContainsKey(property.Name) || this.ContainsKey(displayName)
                }
            ).ToArray();

            // validate required fields
            string[] missingFields = properties.Where(p => p.Required && !p.HasValue).Select(p => p.DisplayName).ToArray();
            if (missingFields.Any())
                throw this.GetMissingKeysException(missingFields);

            // map to model
            foreach (var property in properties.Where(p => p.HasValue))
            {
                // determine which key applies
                string key = this.ContainsKey(property.DisplayName)
                    ? property.DisplayName
                    : property.Name;
                object value = this.Get(key, property.Property.PropertyType, property.Required);
                property.Property.SetValue(model, value);
            }
            return model;
        }

        /// <summary>Get whether the config string is equivalent to a specified config string.</summary>
        /// <param name="other">The other config string to compare.</param>
        public bool EquivalentTo(string other)
        {
            return this.Builder.EquivalentTo(new DbConnectionStringBuilder { ConnectionString = other });
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => (from KeyValuePair<string, object> pair in this.Builder select pair).GetEnumerator();

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        IEnumerator IEnumerable.GetEnumerator() => (from KeyValuePair<string, object> pair in this.Builder select pair).GetEnumerator();


        /***
        ** Write methods
        ***/
        /// <summary>Add an entry to the config string.</summary>
        /// <param name="key">The entry key.</param>
        /// <param name="value">The entry value.</param>
        public void Add(string key, object value)
        {
            this.Builder.Add(key, value);
            this.Builder.ConnectionString = this.Builder.ConnectionString; // workaround for normalisation not being applied on add
        }

        /// <summary>Add multiple entries to the config string builder.</summary>
        /// <param name="values">The values as an anonymous object.</param>
        public void Add(object values)
        {
            foreach (var property in values.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                this.Add(property.Name, property.GetValue(values, null));
        }

        /// <summary>Remove an entry from the config string.</summary>
        /// <param name="key">The entry key.</param>
        public void Remove(string key) => this.Builder.Remove(key);

        /// <summary>Clear the contents of the config string.</summary>
        public void Clear() => this.Builder.Clear();


        /*********
        ** Private methods
        ********/
        /// <summary>Throw an exception indicating the specified keys are not defined.</summary>
        /// <param name="missingKeys">The keys that are not defined.</param>
        private KeyNotFoundException GetMissingKeysException(IEnumerable<string> missingKeys)
        {
            return new KeyNotFoundException($"The config string is invalid. The following fields must be defined: {string.Join(", ", missingKeys)}.");
        }

        /// <summary>Get a value from the config string.</summary>
        /// <param name="key">The entry key.</param>
        /// <param name="type">The expected value type.</param>
        /// <param name="required">Whether to throw an exception if the value is empty or not defined.</param>
        /// <exception cref="KeyNotFoundException">The key was not found and <paramref name="required"/> is <c>true</c>.</exception>
        /// <exception cref="FormatException">The value can't be converted to the requested <paramref name="type"/>.</exception>
        private object Get(string key, Type type, bool required = true)
        {
            // get value
            object value = this.Get(key, required);
            if (value == null)
                return (type.IsValueType ? Activator.CreateInstance(type) : null);
            
            // convert to type
            if (type.IsInstanceOfType(value))
                return value;

            try
            {
                Type underlyingType = Nullable.GetUnderlyingType(type);
                var converter = TypeDescriptor.GetConverter(underlyingType ?? type);
                if (converter.CanConvertFrom(underlyingType ?? type))
                    return converter.ConvertFrom(value);
                if (value is string && converter.CanConvertFrom(typeof (string)))
                    return converter.ConvertFromInvariantString(value as string);
            }
            catch(Exception ex)
            {
                throw new FormatException($"The value '{value}' can't be parsed as type {type.FullName} for field {key}: {ex.Message}");
            }

            throw new FormatException($"The value '{value}' can't be converted to type {type.FullName} for field {key}. There is no conversion available between the two types.");
        }
    }
}
