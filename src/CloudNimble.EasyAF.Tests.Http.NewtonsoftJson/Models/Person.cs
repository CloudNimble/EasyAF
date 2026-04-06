using System;
using System.Text.Json.Serialization;

namespace Microsoft.Restier.Tests.Shared.Scenarios.Library
{

    /// <summary>
    /// 
    /// </summary>
    public class Person
    {

        /// <summary>
        /// 
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

    }

}
