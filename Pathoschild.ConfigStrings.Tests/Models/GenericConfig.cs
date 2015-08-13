using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Pathoschild.ConfigStrings.Tests.Models
{
    /// <summary>A sample configuration model with generic value-type fields.</summary>
    public class GenericConfig<TValue> : ITestableModel where TValue : struct
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A simple value-type property.</summary>
        public TValue Value { get; set; }

        /// <summary>A nullable value-type property.</summary>
        public TValue? Optional { get; set; }

        /// <summary>A nullable value-type property that's required.</summary>
        [Required]
        public TValue? Required { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get a string representation of the model's fields and values.</summary>
        public string GetString()
        {
            return $"{nameof(Value)}=>{this.Value}, {nameof(Optional)}=>{this.Optional}, {nameof(Required)}=>{this.Required}";
        }
    }
}