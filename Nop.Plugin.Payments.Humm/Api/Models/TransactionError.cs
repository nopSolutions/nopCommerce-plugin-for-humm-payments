using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a model for transaction error.
    /// </summary>
    public class TransactionError
    {
        #region Properties

        /// <summary>
        /// Gets or sets the error code in case the API call failed
        /// </summary>
        [JsonProperty("errorId")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the code of the error in case the API call failed
        /// </summary>
        [JsonProperty("errorCode")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the error message in case the API call failed
        /// </summary>
        [JsonProperty("errorMessage")]
        public string Message { get; set; }

        #endregion
    }
}
