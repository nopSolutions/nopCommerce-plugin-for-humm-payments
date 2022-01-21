using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents the API response model for a refund 
    /// </summary>
    public class RefundPaymentResponse : TransactionResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the message result about refund
        /// </summary>
        [JsonProperty("refundMessage")]
        public string Message { get; set; }

        #endregion
    }
}
