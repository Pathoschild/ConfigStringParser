namespace Pathoschild.ConfigStrings.Tests.Models
{
    /// <summary>A simple model which returns a string representation of itself.</summary>
    public interface ITestableModel
    {
        /// <summary>Get a string representation of the model's fields and values.</summary>
        string GetString();
    }
}