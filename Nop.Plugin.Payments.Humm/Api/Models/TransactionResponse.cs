using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a model for the transaction response 
    /// </summary>
    public class TransactionResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to request is successful
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error
        /// </summary>
        [JsonProperty("error")]
        public TransactionError Error { get; set; }

        #endregion
    }
}
