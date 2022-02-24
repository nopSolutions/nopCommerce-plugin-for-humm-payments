using Nop.Core;
using Nop.Core.Domain.Tasks;

namespace Nop.Plugin.Payments.Humm
{
    /// <summary>
    /// Represents a plugin defaults
    /// </summary>
    public static class HummPaymentDefaults
    {
        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "Payments.Humm";

        /// <summary>
        /// Gets the plugin configuration route name
        /// </summary>
        public static string ConfigurationRouteName => "Plugin.Payments.Humm.Configure";

        /// <summary>
        /// Gets the confirm/cancel payment route name
        /// </summary>
        public static string CheckoutCompletedRouteName => "Plugin.Payments.Humm.Completed";

        /// <summary>
        /// Gets the name of the generic attribute that is used to store a tracking ID
        /// </summary>
        public static string TrackingIdAttribute => "Humm.Order.TrackingId";

        /// <summary>
        /// Gets the schedule task to refresh <see cref="HummPaymentSettings.AccessToken"/> and <see cref="HummPaymentSettings.InstanceUrl"/> for store(s).
        /// </summary>
        public static ScheduleTask RefreshPrerequisitesScheduleTask => new()
        {
            Enabled = true,
            Seconds = 60 * 60,
            Name = "Refresh pre-requisites (Humm plugin)",
            Type = "Nop.Plugin.Payments.Humm.Services.RefreshPrerequisitesScheduleTask",
        };

        /// <summary>
        /// Represents a Humm API defaults
        /// </summary>
        public static class API
        {
            /// <summary>
            /// Gets the user agent
            /// </summary>
            public static string UserAgent => $"nopCommerce-{NopVersion.FULL_VERSION}";

            /// <summary>
            /// Gets the default timeout
            /// </summary>
            public static int DefaultTimeout => 20;

            /// <summary>
            /// Gets the datetime format
            /// </summary>
            public static string DatetimeFormat => "yyyy-MM-dd";

            /// <summary>
            /// Represents a endpoints defaults
            /// </summary>
            public class Endpoints
            {
                /// <summary>
                /// Gets the authorize endpoint to get token for sandbox environment
                /// </summary>
                public static string AuthorizeSandbox => "https://test.salesforce.com/services/oauth2/token";

                /// <summary>
                /// Gets the authorize endpoint to get token for production environment
                /// </summary>
                public static string AuthorizeProduction => "https://login.salesforce.com/services/oauth2/token";

                /// <summary>
                /// Gets the initiate process endpoint
                /// </summary>
                public static string InitiatePaymentProcess => "/services/apexrest/initiateProcess";

                /// <summary>
                /// Gets the payment details endpoint
                /// </summary>
                public static string PaymentDetails => "/services/apexrest/getPaymentDetails";

                /// <summary>
                /// Gets the refund payment endpoint
                /// </summary>
                public static string RefundPayment => "/services/apexrest/RefundPayment";
            }
        }
    }
}