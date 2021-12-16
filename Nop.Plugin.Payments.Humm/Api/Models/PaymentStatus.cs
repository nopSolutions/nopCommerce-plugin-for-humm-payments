using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a status of the checkout app in the humm system
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentStatus
    {
        [EnumMember(Value = "TRACKING_ID_NOT_FOUND")]
        TrackingIdNotFound,

        [EnumMember(Value = "NOT_STARTED")]
        NotStarted,

        [EnumMember(Value = "IN_PROGRESS")]
        InProgress,

        [EnumMember(Value = "PAYMENT_DONE")]
        PaymentDone,

        [EnumMember(Value = "PAYMENT_CANCELLED")]
        PaymentCancelled,
    }
}
