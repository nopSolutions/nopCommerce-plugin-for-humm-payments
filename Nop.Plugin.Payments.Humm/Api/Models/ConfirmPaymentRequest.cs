using System;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a model to complete the payment transaction
    /// </summary>
    public class ConfirmPaymentRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to request is successful
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }

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
        /// Gets or sets the error code in case the API call failed
        /// </summary>
        [JsonProperty("errorId")]
        public string ErrorId { get; set; }

        /// <summary>
        /// Gets or sets the code of the error in case the API call failed
        /// </summary>
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the error message in case the API call failed
        /// </summary>
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the payment amount that was paid in the final step of purchase
        /// </summary>
        [JsonProperty("paymentAmount")]
        public decimal? PaymentAmount { get; set; }

        /// <summary>
        /// Gets or sets the date on which payment has been done successfully
        /// </summary>
        [JsonProperty("paymentDate")]
        public DateTime? PaymentDate { get; set; }

        #endregion
    }
}