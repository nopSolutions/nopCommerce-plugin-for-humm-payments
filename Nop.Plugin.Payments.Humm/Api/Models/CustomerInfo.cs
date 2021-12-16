using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a customer info
    /// </summary>
    public class CustomerInfo
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        /// <remarks>
        /// Required parameter
        /// </remarks>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the email
        /// </summary>
        /// <remarks>
        /// Required parameter and should be in proper email format
        /// </remarks>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the mobile number
        /// </summary>
        /// <remarks>
        /// Required parameter, should only contain mobile number without country code 
        /// and must be of 10 digits only
        /// </remarks>
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        #endregion
    }
}
