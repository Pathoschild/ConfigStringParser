using System;

namespace Pathoschild.ConfigStrings.Tests.Models
{
    /// <summary>A sample enumeration value.</summary>
    [Flags]
    public enum SampleEnum
    {
        /// <summary>A simple default value.</summary>
        Zero = 0,

        /// <summary>A simple value.</summary>
        One = 1,

        /// <summary>A simple value.</summary>
        Two = 2,

        /// <summary>A combined flags value.</summary>
        Three = One | Two
    }
}