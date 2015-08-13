using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Pathoschild.ConfigStrings.Tests.Models
{
    /// <summary>A sample configuration model with simple string fields.</summary>
    internal class SimpleConfig : ITestableModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A simple string property with a display name.</summary>
        [DisplayName("Host Name")]
        public string HostName { get; set; }

        /// <summary>A simple string property which is required.</summary>
        [Required]
        public string Required { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get a string representation of the model's fields and values.</summary>
        public string GetString()
        {
            return $"{nameof(HostName)}=>{this.HostName}, {nameof(Required)}=>{this.Required}";
        }
    }
}
