using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a model for API error.
    /// </summary>
    public class ApiError
    {
        #region Properties

        /// <summary>
        /// Gets or sets the code of the error in case the API call failed
        /// </summary>
        [JsonProperty("error")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the error message in case the API call failed
        /// </summary>
        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

        #endregion
    }
}
