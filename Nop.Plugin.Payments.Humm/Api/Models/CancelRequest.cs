namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a model to cancel the payment transaction
    /// </summary>
    public class CancelRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the account id
        /// </summary>
        public string AccId { get; set; }

        /// <summary>
        /// Gets or sets the order tracking id
        /// </summary>
        public string OrderId { get; set; }

        #endregion
    }
}