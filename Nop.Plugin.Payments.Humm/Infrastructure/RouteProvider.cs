using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Humm.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        #region Properties

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -1;

        #endregion

        #region Methods

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(HummPaymentDefaults.ConfirmPaymentRouteName, "humm/confirm",
                new { controller = "HummPayment", action = "Confirm" });

            endpointRouteBuilder.MapControllerRoute(HummPaymentDefaults.CancelPaymentRouteName, "humm/cancel",
                new { controller = "HummPayment", action = "Cancel" });
        }

        #endregion
    }
}
