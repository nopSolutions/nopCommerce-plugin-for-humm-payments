using System;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a checkout exception
    /// </summary>
    public class CheckoutException : Exception
    {
        public CheckoutException(string trackingId, string message) : base(message)
        {
            TrackingId = trackingId;
        }

        /// <summary>
        /// Gets or sets the tracking id
        /// </summary>
        public string TrackingId { get; set; }
    }
}