using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a model to initiate payment process
    /// </summary>
    public class InitiateProcessRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the account ID assigned by humm (will vary with environment).
        /// </summary>
        /// <remarks>
        /// Required parameter
        /// </remarks>
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the customer info
        /// </summary>
        /// <remarks>
        /// Required parameter
        /// </remarks>
        [JsonProperty("customerInfo")]
        public CustomerInfo CustomerInfo { get; set; }

        /// <summary>
        /// Gets or sets the order details
        /// </summary>
        /// <remarks>
        /// Required parameter
        /// </remarks>
        [JsonProperty("orderDetails")]
        public OrderDetails OrderDetails { get; set; }

        #endregion
    }
}
