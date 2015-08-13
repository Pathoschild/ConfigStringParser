using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Pathoschild.ConfigStrings.Tests.Models;

namespace Pathoschild.ConfigStrings.Tests
{
    /// <summary>Unit tests for the <see cref="ConfigStringParser"/> class.</summary>
    [TestFixture]
    public class ConfigStringParserTests
    {
        /*********
        ** Test cases
        *********/
        /***
        ** Fields
        ****/
        /// <summary>Assert that <see cref="ConfigStringParser.Count"/> returns the expected number for various config strings.</summary>
        /// <param name="configString">The config string to parse.</param>
        [Test]
        [TestCase("", ExpectedResult = 0)]
        [TestCase("Host Name=", ExpectedResult = 0)]
        [TestCase("Host Name=example.com", ExpectedResult = 1)]
        [TestCase("Host Name=example.com;", ExpectedResult = 1)]
        [TestCase("Host Name=example.com; Port=587; EnableSSL = true", ExpectedResult = 3)]
        [TestCase("Host Name=example.com; Port=587; EnableSSL = true; Weird = \"String with special=characters!;\"", ExpectedResult = 4)]
        public int Field_Count(string configString)
        {
            return new ConfigStringParser(configString).Count;
        }

        /// <summary>Assert that <see cref="ConfigStringParser.Keys"/> returns the expected keys for various config strings.</summary>
        /// <param name="configString">The config string to parse.</param>
        [Test]
        [TestCase("", ExpectedResult = "")]
        [TestCase("Host Name=", ExpectedResult = "")]
        [TestCase("Host Name=example.com", ExpectedResult = "host name")]
        [TestCase("Host Name=example.com;", ExpectedResult = "host name")]
        [TestCase("Host Name=example.com; Port=587; EnableSSL = true", ExpectedResult = "enablessl,host name,port")]
        [TestCase("Host Name=example.com; Port=587; EnableSSL = true; Weird = \"String with special=characters!;\"", ExpectedResult = "enablessl,host name,port,weird")]
        public string Field_Keys(string configString)
        {
            IEnumerable<string> keys = new ConfigStringParser(configString).Keys;
            return keys != null
                ? string.Join(",", keys.OrderBy(p => p))
                : null;
        }

        /// <summary>Assert that <see cref="ConfigStringParser.ConfigString"/> returns a normalised config string.</summary>
        /// <param name="configString">The config string to parse.</param>
        [Test]
        [TestCase("", ExpectedResult = "")]
        [TestCase("Host Name=", ExpectedResult = "")]
        [TestCase("Host Name=example.com", ExpectedResult = "host name=example.com")]
        [TestCase("Host Name=example.com;", ExpectedResult = "host name=example.com")]
        [TestCase("Host Name=example.com; Port=587; EnableSSL = true", ExpectedResult = "host name=example.com;port=587;enablessl=true")]
        [TestCase("Host Name=example.com; Port=587; EnableSSL = true; Weird = \"String with special=characters!;\"", ExpectedResult = "host name=example.com;port=587;enablessl=true;weird=\"String with special=characters!;\"")]
        public string Field_ConfigString(string configString)
        {
            return new ConfigStringParser(configString).ConfigString;
        }

        /// <summary>Assert that <see cref="ConfigStringParser.ConfigString"/> returns a normalised config string.</summary>
        /// <param name="configString">The config string to parse.</param>
        /// <param name="key">The key to get.</param>
        [Test]
        [TestCase("", "host name", ExpectedResult = null)]
        [TestCase("Host Name=", "host name", ExpectedResult = null)]
        [TestCase("Host Name=example.com", "host name", ExpectedResult = "example.com")]
        [TestCase("Host Name=example.com", "HOsT nAmE", ExpectedResult = "example.com")]
        [TestCase("Host Name=example.com;", "host name", ExpectedResult = "example.com")]
        [TestCase("Host Name=example.com; Port=587; EnableSSL = true", "port", ExpectedResult = "587")]
        [TestCase("Host Name=example.com; Port=587; EnableSSL = true; Weird = \"String with special=characters!;\"", "weird", ExpectedResult = "String with special=characters!;")]
        public object Field_Indexer(string configString, string key)
        {
            return new ConfigStringParser(configString)[key];
        }

        /***
        ** Read methods
        ****/
        /// <summary>Assert that <see cref="ConfigStringParser.ContainsKey"/> returns the expected value for various config strings.</summary>
        /// <param name="configString">The config string to parse.</param>
        /// <param name="key">The key to get.</param>
        [Test]
        [TestCase("", "host name", ExpectedResult = false)]
        [TestCase("Host Name=", "host name", ExpectedResult = false)]
        [TestCase("Host Name=example.com;", "missing key", ExpectedResult = false)]
        [TestCase("Host Name=example.com", "host name", ExpectedResult = true)]
        [TestCase("Host Name=example.com", "HOsT nAmE", ExpectedResult = true)]
        [TestCase("Host Name=example.com;", "host name", ExpectedResult = true)]
        [TestCase("Host Name=example.com; Port=587; EnableSSL = true; Weird = \"String with special=characters!;\"", "weird", ExpectedResult = true)]
        public bool GetMethod_ContainsKey(string configString, string key)
        {
            return new ConfigStringParser(configString).ContainsKey(key);
        }

        /// <summary>Assert that <see cref="ConfigStringParser.Get(string,bool)"/> returns the expected value when the 'required' parameter is <c>true</c>.</summary>
        /// <param name="configString">The config string to parse.</param>
        /// <param name="key">The key to get.</param>
        [Test]
        [TestCase("", "host name", ExpectedException = typeof(KeyNotFoundException))]
        [TestCase("Host Name=", "host name", ExpectedException = typeof(KeyNotFoundException))]
        [TestCase("Host Name=example.com", "host name", ExpectedResult = "example.com")]
        [TestCase("Host Name=example.com", "HOsT nAmE", ExpectedResult = "example.com")]
        [TestCase("Host Name=example.com;", "host name", ExpectedResult = "example.com")]
        public object GetMethod_Get_RequiredWithoutType(string configString, string key)
        {
            return new ConfigStringParser(configString).Get(key, required: true);
        }

        /// <summary>Assert that <see cref="ConfigStringParser.Get(string,bool)"/> returns the expected value when the 'required' parameter is <c>false</c>.</summary>
        /// <param name="configString">The config string to parse.</param>
        /// <param name="key">The key to get.</param>
        [Test]
        [TestCase("", "host name", ExpectedResult = null)]
        [TestCase("Host Name=", "host name", ExpectedResult = null)]
        [TestCase("Host Name=example.com", "host name", ExpectedResult = "example.com")]
        [TestCase("Host Name=example.com", "HOsT nAmE", ExpectedResult = "example.com")]
        [TestCase("Host Name=example.com;", "host name", ExpectedResult = "example.com")]
        public object GetMethod_Get_OptionalWithoutType(string configString, string key)
        {
            return new ConfigStringParser(configString).Get(key, required: false);
        }

        /// <summary>Assert that <see cref="ConfigStringParser.Get(string,bool)"/> returns the expected value when the 'required' parameter is <c>true</c>.</summary>
        /// <param name="configString">The config string to parse.</param>
        /// <param name="key">The key to get.</param>
        /// <param name="type">The expected value type.</param>
        [Test]
        [TestCase("", "host name", typeof(string), ExpectedException = typeof(KeyNotFoundException))]
        [TestCase("", "host name", typeof(int), ExpectedException = typeof(KeyNotFoundException))]
        [TestCase("Host Name=", "host name", typeof(string), ExpectedException = typeof(KeyNotFoundException))]
        [TestCase("Host Name=example.com", "host name", typeof(string), ExpectedResult = "example.com")]
        [TestCase("Host Name=example.com", "HOsT nAmE", typeof(string), ExpectedResult = "example.com")]
        [TestCase("Host Name=example.com;", "host name", typeof(string), ExpectedResult = "example.com")]
        [TestCase("Host Name=example.com;", "host name", typeof(int), ExpectedException = typeof(FormatException))]
        [TestCase("Host Name=example.com; Port=587", "port", typeof(int), ExpectedResult = 587)]
        public object GetMethod_Get_RequiredWithType(string configString, string key, Type type)
        {
            var method = typeof(ConfigStringParser)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(p => p.Name == nameof(ConfigStringParser.Get) && p.IsGenericMethod)
                .MakeGenericMethod(type);

            try
            {
                return method.Invoke(new ConfigStringParser(configString), new object[] { key, true });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
        }

        /// <summary>Assert that <see cref="ConfigStringParser.Get(string,bool)"/> returns the expected value when the 'required' parameter is <c>true</c>.</summary>
        /// <param name="configString">The config string to parse.</param>
        /// <param name="key">The key to get.</param>
        /// <param name="type">The expected value type.</param>
        [Test]
        [TestCase("", "host name", typeof(string), ExpectedResult = null)]
        [TestCase("", "host name", typeof(int), ExpectedResult = 0)]
        [TestCase("Host Name=", "host name", typeof(string), ExpectedResult = null)]
        [TestCase("Host Name=example.com", "host name", typeof(string), ExpectedResult = "example.com")]
        [TestCase("Host Name=example.com", "HOsT nAmE", typeof(string), ExpectedResult = "example.com")]
        [TestCase("Host Name=example.com;", "host name", typeof(string), ExpectedResult = "example.com")]
        [TestCase("Host Name=example.com;", "host name", typeof(int), ExpectedException = typeof(FormatException))]
        [TestCase("Host Name=example.com; Port=587", "port", typeof(int), ExpectedResult = 587)]
        public object GetMethod_Get_OptionalWithType(string configString, string key, Type type)
        {
            MethodInfo method = typeof(ConfigStringParser)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(p => p.Name == nameof(ConfigStringParser.Get) && p.IsGenericMethod)
                .MakeGenericMethod(type);

            try
            {
                return method.Invoke(new ConfigStringParser(configString), new object[] { key, false });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
        }

        /// <summary>Assert that <see cref="ConfigStringParser.AssertRequiredKeys"/> raises the expected exceptions.</summary>
        /// <param name="configString">The config string to parse.</param>
        /// <param name="keys">The keys to assert.</param>
        [Test]
        [TestCase("", new[] { "host name", "missing" }, ExpectedException = typeof(KeyNotFoundException), ExpectedMessage = "The config string is invalid. The following fields must be defined: host name, missing.")]
        [TestCase("host name=", new[] { "host name", "missing" }, ExpectedException = typeof(KeyNotFoundException), ExpectedMessage = "The config string is invalid. The following fields must be defined: host name, missing.")]
        [TestCase("host name=example.com", new[] { "host name", "missing" }, ExpectedException = typeof(KeyNotFoundException), ExpectedMessage = "The config string is invalid. The following fields must be defined: missing.")]
        [TestCase("host name=example.com; port=587", new[] { "host name", "port" })]
        public void GetMethod_AssertRequiredKeys(string configString, string[] keys)
        {
            new ConfigStringParser(configString).AssertRequiredKeys(keys);
        }

        /// <summary>Assert that <see cref="ConfigStringParser.EquivalentTo"/> matches other config strings as expected.</summary>
        /// <param name="configString">The config string to parse.</param>
        /// <param name="otherConfigString">The other config string to compare.</param>
        [Test]
        [TestCase("", "", ExpectedResult = true)]
        [TestCase("host name=", "", ExpectedResult = true)]
        [TestCase("host name=; PORT = 587", "port = 587;", ExpectedResult = true)]
        [TestCase("host name=example.com; PORT = 587", "port = 587; host NAmE=example.com", ExpectedResult = true)]
        [TestCase("host name=EXAMPLE.com; port=587", "port=587; host name=example.com", ExpectedResult = false)]
        [TestCase("host name=EXAMPLE.com; port=587", "host name=example.com", ExpectedResult = false)]
        [TestCase("host name=EXAMPLE.com; port=587", "", ExpectedResult = false)]
        [TestCase("host name=EXAMPLE.com; port=587", null, ExpectedResult = false)]
        public bool GetMethod_EquivalentTo(string configString, string otherConfigString)
        {
            return new ConfigStringParser(configString).EquivalentTo(otherConfigString);
        }

        /// <summary>Assert that <see cref="ConfigStringParser.GetEnumerator"/> returns the expected key/value pairs.</summary>
        /// <param name="configString">The config string to parse.</param>
        [Test]
        [TestCase("", ExpectedResult = new string[0])]
        [TestCase("host name=", ExpectedResult = new string[0])]
        [TestCase("host name=; PORT = 587", ExpectedResult = new[] { "port=>587" })]
        [TestCase("host NaMe=example.com; PORT = 587", ExpectedResult = new[] { "host name=>example.com", "port=>587" })]
        [TestCase("host NaMe=example.com; PORT = 587;  Weird = \"String with special=characters!;\"", ExpectedResult = new[] { "host name=>example.com", "port=>587", "weird=>String with special=characters!;" })]
        public string[] GetMethod_GetEnumerator(string configString)
        {
            return new ConfigStringParser(configString)
                .Select(p => $"{p.Key}=>{p.Value}")
                .ToArray();
        }

        /***
        ** Read to model methods
        ***/
        /// <summary>Assert that <see cref="ConfigStringParser.MapTo{TModel}()"/> correctly populates the target entity.</summary>
        /// <param name="configString">The config string to parse.</param>
        [Test]
        [TestCase("", ExpectedException = typeof(KeyNotFoundException), ExpectedMessage = "The config string is invalid. The following fields must be defined: Required.")]
        [TestCase("Required=arbitrary text", ExpectedResult = "HostName=>, Required=>arbitrary text")]
        [TestCase("Host Name=;Required=arbitrary text", ExpectedResult = "HostName=>, Required=>arbitrary text")]
        [TestCase("Host NAME=example.com;Required=arbitrary text", ExpectedResult = "HostName=>example.com, Required=>arbitrary text")]
        [TestCase("HostName=example.com;Required=arbitrary text", ExpectedResult = "HostName=>example.com, Required=>arbitrary text")]
        [TestCase("HoStNaMe=example.com;Required=arbitrary text", ExpectedResult = "HostName=>example.com, Required=>arbitrary text")]
        public string WriteMethod_MapTo_Strings(string configString)
        {
            return new ConfigStringParser(configString).MapTo<SimpleConfig>().GetString();
        }

        /// <summary>Assert that <see cref="ConfigStringParser.MapTo{TModel}()"/> correctly populates the target entity with primitive types.</summary>
        /// <param name="configString">The config string to parse.</param>
        /// <param name="type">The value type.</param>
        [Test]
        [TestCase("", typeof(bool), ExpectedException = typeof(KeyNotFoundException), ExpectedMessage = "The config string is invalid. The following fields must be defined: Required.")]
        [TestCase("Required=true", typeof(bool), ExpectedResult = "Value=>False, Optional=>, Required=>True")]
        [TestCase("Required=false", typeof(bool), ExpectedResult = "Value=>False, Optional=>, Required=>False")]
        [TestCase("Required=true;Optional=", typeof(bool), ExpectedResult = "Value=>False, Optional=>, Required=>True")]
        [TestCase("Required=true;Optional=true", typeof(bool), ExpectedResult = "Value=>False, Optional=>True, Required=>True")]
        [TestCase("Required=true;Optional=true;Value=", typeof(bool), ExpectedResult = "Value=>False, Optional=>True, Required=>True")]
        [TestCase("Required=true;Optional=true;Value=true", typeof(bool), ExpectedResult = "Value=>True, Optional=>True, Required=>True")]
        [TestCase("", typeof(int), ExpectedException = typeof(KeyNotFoundException), ExpectedMessage = "The config string is invalid. The following fields must be defined: Required.")]
        [TestCase("Required=14", typeof(int), ExpectedResult = "Value=>0, Optional=>, Required=>14")]
        [TestCase("Required=14;Optional=", typeof(int), ExpectedResult = "Value=>0, Optional=>, Required=>14")]
        [TestCase("Required=14;Optional=15", typeof(int), ExpectedResult = "Value=>0, Optional=>15, Required=>14")]
        [TestCase("Required=14;Optional=15;Value=", typeof(int), ExpectedResult = "Value=>0, Optional=>15, Required=>14")]
        [TestCase("Required=14;Optional=15;Value=16", typeof(int), ExpectedResult = "Value=>16, Optional=>15, Required=>14")]
        [TestCase("", typeof(short), ExpectedException = typeof(KeyNotFoundException), ExpectedMessage = "The config string is invalid. The following fields must be defined: Required.")]
        [TestCase("Required=14", typeof(short), ExpectedResult = "Value=>0, Optional=>, Required=>14")]
        [TestCase("Required=14;Optional=", typeof(short), ExpectedResult = "Value=>0, Optional=>, Required=>14")]
        [TestCase("Required=14;Optional=15", typeof(short), ExpectedResult = "Value=>0, Optional=>15, Required=>14")]
        [TestCase("Required=14;Optional=15;Value=", typeof(short), ExpectedResult = "Value=>0, Optional=>15, Required=>14")]
        [TestCase("Required=14;Optional=15;Value=16", typeof(short), ExpectedResult = "Value=>16, Optional=>15, Required=>14")]
        [TestCase("", typeof(double), ExpectedException = typeof(KeyNotFoundException), ExpectedMessage = "The config string is invalid. The following fields must be defined: Required.")]
        [TestCase("Required=14.5", typeof(double), ExpectedResult = "Value=>0, Optional=>, Required=>14.5")]
        [TestCase("Required=14.5;Optional=", typeof(double), ExpectedResult = "Value=>0, Optional=>, Required=>14.5")]
        [TestCase("Required=14.5;Optional=15", typeof(double), ExpectedResult = "Value=>0, Optional=>15, Required=>14.5")]
        [TestCase("Required=14.5;Optional=15;Value=", typeof(double), ExpectedResult = "Value=>0, Optional=>15, Required=>14.5")]
        [TestCase("Required=14.5;Optional=15;Value=16", typeof(double), ExpectedResult = "Value=>16, Optional=>15, Required=>14.5")]
        [TestCase("", typeof(long), ExpectedException = typeof(KeyNotFoundException), ExpectedMessage = "The config string is invalid. The following fields must be defined: Required.")]
        [TestCase("Required=14", typeof(long), ExpectedResult = "Value=>0, Optional=>, Required=>14")]
        [TestCase("Required=14;Optional=", typeof(long), ExpectedResult = "Value=>0, Optional=>, Required=>14")]
        [TestCase("Required=14;Optional=15", typeof(long), ExpectedResult = "Value=>0, Optional=>15, Required=>14")]
        [TestCase("Required=14;Optional=15;Value=", typeof(long), ExpectedResult = "Value=>0, Optional=>15, Required=>14")]
        [TestCase("Required=14;Optional=15;Value=16", typeof(long), ExpectedResult = "Value=>16, Optional=>15, Required=>14")]
        [TestCase("", typeof(Guid), ExpectedException = typeof(KeyNotFoundException), ExpectedMessage = "The config string is invalid. The following fields must be defined: Required.")]
        [TestCase("Required=95a23cad-5ad6-4ab4-bb0d-8079f062ac2c", typeof(Guid), ExpectedResult = "Value=>00000000-0000-0000-0000-000000000000, Optional=>, Required=>95a23cad-5ad6-4ab4-bb0d-8079f062ac2c")]
        [TestCase("Required=95a23cad-5ad6-4ab4-bb0d-8079f062ac2c;Optional=", typeof(Guid), ExpectedResult = "Value=>00000000-0000-0000-0000-000000000000, Optional=>, Required=>95a23cad-5ad6-4ab4-bb0d-8079f062ac2c")]
        [TestCase("Required=95a23cad-5ad6-4ab4-bb0d-8079f062ac2c;Optional=95a23cad-5ad6-4ab4-bb0d-8079f062ac2d", typeof(Guid), ExpectedResult = "Value=>00000000-0000-0000-0000-000000000000, Optional=>95a23cad-5ad6-4ab4-bb0d-8079f062ac2d, Required=>95a23cad-5ad6-4ab4-bb0d-8079f062ac2c")]
        [TestCase("Required=95a23cad-5ad6-4ab4-bb0d-8079f062ac2c;Optional=95a23cad-5ad6-4ab4-bb0d-8079f062ac2d;Value=", typeof(Guid), ExpectedResult = "Value=>00000000-0000-0000-0000-000000000000, Optional=>95a23cad-5ad6-4ab4-bb0d-8079f062ac2d, Required=>95a23cad-5ad6-4ab4-bb0d-8079f062ac2c")]
        [TestCase("Required=95a23cad-5ad6-4ab4-bb0d-8079f062ac2c;Optional=95a23cad-5ad6-4ab4-bb0d-8079f062ac2d;Value=95a23cad-5ad6-4ab4-bb0d-8079f062ac2e", typeof(Guid), ExpectedResult = "Value=>95a23cad-5ad6-4ab4-bb0d-8079f062ac2e, Optional=>95a23cad-5ad6-4ab4-bb0d-8079f062ac2d, Required=>95a23cad-5ad6-4ab4-bb0d-8079f062ac2c")]
        public string WriteMethod_MapTo_PrimitiveTypes(string configString, Type type)
        {
            Type modelType = typeof(GenericConfig<>).MakeGenericType(type);
            MethodInfo method = typeof(ConfigStringParser)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(p => p.Name == nameof(ConfigStringParser.MapTo) && !p.GetParameters().Any())
                .MakeGenericMethod(modelType);

            try
            {
                ITestableModel model = (ITestableModel)method.Invoke(new ConfigStringParser(configString), null);
                return model.GetString();
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
        }

        /// <summary>Assert that <see cref="ConfigStringParser.MapTo{TModel}()"/> correctly populates the target entity with enum values.</summary>
        /// <param name="configString">The config string to parse.</param>
        [Test]
        [TestCase("", ExpectedException = typeof(KeyNotFoundException), ExpectedMessage = "The config string is invalid. The following fields must be defined: Required.")]
        [TestCase("Required=One", ExpectedResult = "Value=>Zero, Optional=>, Required=>One")]
        [TestCase("Required=One;Optional=", ExpectedResult = "Value=>Zero, Optional=>, Required=>One")]
        [TestCase("Required=One;Optional=One,Two", ExpectedResult = "Value=>Zero, Optional=>Three, Required=>One")]
        [TestCase("Required=One;Optional=Three", ExpectedResult = "Value=>Zero, Optional=>Three, Required=>One")]
        [TestCase("Required=One;Optional=Two;Value=Three", ExpectedResult = "Value=>Three, Optional=>Two, Required=>One")]
        public string WriteMethod_MapTo_Enums(string configString)
        {
            return new ConfigStringParser(configString).MapTo<GenericConfig<SampleEnum>>().GetString();
        }

        /***
        ** Write methods
        ****/
        /// <summary>Assert that <see cref="ConfigStringParser.Add(string,object)"/> modifies the <see cref="ConfigStringParser.ConfigString"/> in the expected way.</summary>
        /// <param name="configString">The config string to parse.</param>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        [Test]
        [TestCase("Host name=example.com", "Added", "string", ExpectedResult = "host name=example.com;added=string")]
        [TestCase("Host name=example.com", "Added", "string with spaces", ExpectedResult = "host name=example.com;added=\"string with spaces\"")]
        [TestCase("Host name=example.com", "Added", "string-with-special=characters!;", ExpectedResult = "host name=example.com;added=\"string-with-special=characters!;\"")]
        [TestCase("Host name=example.com", "Added", 42, ExpectedResult = "host name=example.com;added=42")]
        [TestCase("Host name=example.com", "Added", 42.0, ExpectedResult = "host name=example.com;added=42")]
        public string WriteMethod_Add_WithKeyValue_ReturnsExpectedConfigString(string configString, string key, object value)
        {
            return new ConfigStringParser(configString) { { key, value } }.ConfigString;
        }

        /// <summary>Assert that <see cref="ConfigStringParser.Add(object)"/> modifies the <see cref="ConfigStringParser.ConfigString"/> in the expected way.</summary>
        /// <param name="configString">The config string to parse.</param>
        /// <param name="value">The value to add.</param>
        [Test]
        [TestCase("Host name=example.com", "string", ExpectedResult = "host name=example.com;added=string")]
        [TestCase("Host name=example.com", "string with spaces", ExpectedResult = "host name=example.com;added=\"string with spaces\"")]
        [TestCase("Host name=example.com", "string-with-special=characters!;", ExpectedResult = "host name=example.com;added=\"string-with-special=characters!;\"")]
        [TestCase("Host name=example.com", 42, ExpectedResult = "host name=example.com;added=42")]
        [TestCase("Host name=example.com", 42.0, ExpectedResult = "host name=example.com;added=42")]
        public string WriteMethod_Add_WithAnonymousObject_ReturnsExpectedConfigString(string configString, object value)
        {
            var parser = new ConfigStringParser(configString);
            parser.Add(new { added = value });
            return parser.ConfigString;
        }

        /// <summary>Assert that <see cref="ConfigStringParser.Remove"/> modifies the <see cref="ConfigStringParser.ConfigString"/> in the expected way.</summary>
        /// <param name="configString">The config string to parse.</param>
        /// <param name="key">The key to remove.</param>
        [Test]
        [TestCase("Host name=example.com", "Host Name", ExpectedResult = "")]
        [TestCase("Host name=example.com", "HoSt NaMe", ExpectedResult = "")]
        [TestCase("Host Name=example.com; Port=587; EnableSSL = true", "port", ExpectedResult = "host name=example.com;enablessl=true")]
        public string WriteMethod_Remove_ReturnsExpectedConfigString(string configString, string key)
        {
            var parser = new ConfigStringParser(configString);
            parser.Remove(key);
            return parser.ConfigString;
        }

        /// <summary>Assert that <see cref="ConfigStringParser.Remove"/> modifies the <see cref="ConfigStringParser.ConfigString"/> in the expected way.</summary>
        /// <param name="configString">The config string to parse.</param>
        [Test]
        [TestCase("", ExpectedResult = "")]
        [TestCase("Host name=", ExpectedResult = "")]
        [TestCase("Host name=example.com", ExpectedResult = "")]
        [TestCase("Host name=example.com", ExpectedResult = "")]
        [TestCase("Host Name=example.com; Port=587; EnableSSL = true", ExpectedResult = "")]
        public string WriteMethod_Clear_ReturnsExpectedConfigString(string configString)
        {
            var parser = new ConfigStringParser(configString);
            parser.Clear();
            return parser.ConfigString;
        }
    }
}
