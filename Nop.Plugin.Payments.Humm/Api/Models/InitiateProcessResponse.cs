using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a model for the initiate process API response
    /// </summary>
    public class InitiateProcessResponse : TransactionResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the flexi tracking ID
        /// </summary>
        /// <remarks>
        /// Required parameter and must be of 16 in length
        /// </remarks>
        [JsonProperty("flexiTrackingId")]
        public string FlexiTrackingId { get; set; }

        /// <summary>
        /// Gets or sets the partner tracking ID
        /// </summary>
        /// <remarks>
        /// Required parameter
        /// </remarks>
        [JsonProperty("partnerTrackingId")]
        public string PartnerTrackingId { get; set; }

        /// <summary>
        /// Gets or sets the redirect URL
        /// </summary>
        /// <remarks>
        /// Required parameter
        /// </remarks>
        [JsonProperty("redirectURL")]
        public string RedirectUrl { get; set; }

        #endregion
    }
}
