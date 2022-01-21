using System;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a model for the payment details API response
    /// </summary>
    public class GetPaymentDetailsResponse : TransactionResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the current status of the checkout app in the humm system
        /// </summary>
        /// <remarks>
        /// Required parameter
        /// </remarks>
        [JsonProperty("status")]
        public PaymentStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the amount of which payment has been made by the user
        /// </summary>
        /// <remarks>
        /// Required parameter (if Checkout has been completed at humm end)
        /// </remarks>
        [JsonProperty("paymentAmount")]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the date on which payment has been done
        /// </summary>
        /// <remarks>
        /// Required parameter (if Checkout has been completed at humm end) and must of format YYYY-MM-DD)
        /// </remarks>
        [JsonProperty("paymentDate")]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Gets or sets the cancellation reason of Checkout, if the checkout app has been cancelled
        /// </summary>
        /// <remarks>
        /// Required parameter (if Checkout has been cancelled at humm end)
        /// </remarks>
        [JsonProperty("cancellationReason")]
        public string CancellationReason { get; set; }

        /// <summary>
        /// Cancellation date of Checkout, if the checkout app has been cancelled.
        /// </summary>
        /// <remarks>
        /// Required parameter Required (if Checkout has been cancelled at humm end) and must of format YYYY-MM-DD)
        /// </remarks>
        [JsonProperty("cancellationDate")]
        public DateTime? CancellationDate { get; set; }

        #endregion
    }
}
