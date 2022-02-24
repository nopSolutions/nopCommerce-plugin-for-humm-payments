using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Humm.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        #region Methods

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(name: HummPaymentDefaults.ConfigurationRouteName,
                pattern: "Admin/Humm/Configuration",
                defaults: new { controller = "HummConfiguration", action = "Configure", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: HummPaymentDefaults.CheckoutCompletedRouteName,
                pattern: "humm/checkout/{token:guid}",
                defaults: new { controller = "HummPayment", action = "CheckoutCompleted" });
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;

        #endregion
    }
}