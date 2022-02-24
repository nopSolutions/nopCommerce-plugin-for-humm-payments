using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a status of the checkout app in the humm system
    /// </summary>
    public enum PaymentStatus
    {
        [EnumMember(Value = "TRACKING_ID_NOT_FOUND")]
        NotFound,

        [EnumMember(Value = "NOT_STARTED")]
        NotStarted,

        [EnumMember(Value = "IN_PROGRESS")]
        InProgress,

        [EnumMember(Value = "PAYMENT_DONE")]
        Completed,

        [EnumMember(Value = "PAYMENT_CANCELLED")]
        Cancelled,

        [EnumMember(Value = "CUSTOMER_REFUNDED")]
        Refunded,

        [EnumMember(Value = "REFUND_IN_PROGRESS")]
        RefundInProgress,

        [EnumMember(Value = "PARTIAL_REFUND_COMPLETED")]
        PartiallyRefunded,

        [EnumMember(Value = "FULL_REFUND_REQUESTED")]
        RefundRequested
    }
}