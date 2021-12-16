using System;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a customer info
    /// </summary>
    public class OrderDetails
    {
        #region Properties

        /// <summary>
        /// Gets or sets the invoice ID related to the Order
        /// </summary>
        /// <remarks>
        /// Optional parameter
        /// </remarks>
        [JsonProperty("orderInvoiceID")]
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the description of the goods being purchases
        /// </summary>
        /// <remarks>
        /// Required parameter
        /// </remarks>
        [JsonProperty("orderDescription")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the total amount of the goods being purchased
        /// </summary>
        /// <remarks>
        /// Required parameter
        /// </remarks>
        [JsonProperty("orderInvoiceAmt")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the date when the order was placed
        /// </summary>
        /// <remarks>
        /// Required parameter and must of format YYYY-MM-DD
        /// </remarks>
        [JsonProperty("orderDate")]
        public DateTime Date { get; set; }

        #endregion
    }
}
