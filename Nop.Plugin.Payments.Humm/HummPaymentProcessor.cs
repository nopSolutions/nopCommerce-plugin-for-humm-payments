using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Humm.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Payments.Humm
{
    public class HummPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly HummPaymentSettings _hummPaymentSettings;
        private readonly HummService _hummService;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IPaymentService _paymentService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public HummPaymentProcessor(
            HummPaymentSettings hummPaymentSettings,
            HummService hummService,
            IActionContextAccessor actionContextAccessor,
            ILocalizationService localizationService,
            ILogger logger,
            IPaymentService paymentService,
            INotificationService notificationService,
            IUrlHelperFactory urlHelperFactory,
            ISettingService settingService,
            IScheduleTaskService scheduleTaskService,
            IWebHelper webHelper
        )
        {
            _hummPaymentSettings = hummPaymentSettings;
            _hummService = hummService;
            _actionContextAccessor = actionContextAccessor;
            _localizationService = localizationService;
            _logger = logger;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _urlHelperFactory = urlHelperFactory;
            _settingService = settingService;
            _scheduleTaskService = scheduleTaskService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>The <see cref="Task"/> containing the <see cref="ProcessPaymentResult"/></returns>
        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            if (postProcessPaymentRequest is null)
                throw new ArgumentNullException(nameof(postProcessPaymentRequest));

            var order = postProcessPaymentRequest.Order;
            var (success, redirectUrl, errors) = await _hummService.CreatePaymentAsync(order);
            if (success)
                _actionContextAccessor.ActionContext.HttpContext.Response.Redirect(redirectUrl);
            else
            {
                await _logger.ErrorAsync($"{HummPaymentDefaults.SystemName}: Error when creating the Humm payment transaction for order " +
                    $"#{order.CustomOrderNumber}. {string.Join(Environment.NewLine, errors)}");

                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                var failUrl = urlHelper.RouteUrl("OrderDetails", new { orderId = order.Id }, _webHelper.GetCurrentRequestProtocol());

                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Payments.Humm.InvalidPayment"));

                _actionContextAccessor.ActionContext.HttpContext.Response.Redirect(failUrl);
            }
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>The <see cref="Task"/> containing a value indicating whether payment method should be hidden during checkout</returns>
        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(!_hummService.IsConfigured());
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>The <see cref="Task"/> containing a additional handling fee</returns>
        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _paymentService.CalculateAdditionalFeeAsync(cart,
                _hummPaymentSettings.AdditionalFee, _hummPaymentSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>The <see cref="Task"/> containing the <see cref="CapturePaymentResult"/></returns>
        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>The <see cref="Task"/> containing the <see cref="RefundPaymentResult"/></returns>
        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            if (refundPaymentRequest is null)
                throw new ArgumentNullException(nameof(refundPaymentRequest));

            var order = refundPaymentRequest.Order;
            var (success, errors) = await _hummService.RefundOrderAsync(order, refundPaymentRequest.AmountToRefund);
            if (!success)
                return new RefundPaymentResult { Errors = errors };

            return new RefundPaymentResult
            {
                NewPaymentStatus = refundPaymentRequest.IsPartialRefund
                    ? PaymentStatus.PartiallyRefunded
                    : PaymentStatus.Refunded
            };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>The <see cref="Task"/> containing the <see cref="VoidPaymentResult"/></returns>
        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>The <see cref="Task"/> containing the <see cref="ProcessPaymentResult"/></returns>
        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>The <see cref="Task"/> containing the <see cref="CancelRecurringPaymentResult"/></returns>
        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>The <see cref="Task"/> containing a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)</returns>
        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!_hummService.IsConfigured())
                return Task.FromResult(false);

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>The <see cref="Task"/> containing the list of validating errors</returns>
        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>The <see cref="Task"/> containing the payment info holder</returns>
        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory
                .GetUrlHelper(_actionContextAccessor.ActionContext)
                .RouteUrl(HummPaymentDefaults.ConfigurationRouteName);
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return null;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new HummPaymentSettings
            {
                IsSandbox = true,
                LogIpnErrors = true
            });

            if (await _scheduleTaskService.GetTaskByTypeAsync(HummPaymentDefaults.RefreshPrerequisitesScheduleTask.Type) is null)
                await _scheduleTaskService.InsertTaskAsync(HummPaymentDefaults.RefreshPrerequisitesScheduleTask);

            await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Payments.Humm.Fields.IsSandbox"] = "Use sandbox",
                ["Plugins.Payments.Humm.Fields.IsSandbox.Hint"] = "Determine whether to use the sandbox environment for testing purposes.",
                ["Plugins.Payments.Humm.Fields.SandboxAccountId"] = "Sandbox account ID",
                ["Plugins.Payments.Humm.Fields.SandboxAccountId.Hint"] = "Enter the account ID provided by Humm for sandbox environment.",
                ["Plugins.Payments.Humm.Fields.ProductionAccountId"] = "Production account ID",
                ["Plugins.Payments.Humm.Fields.ProductionAccountId.Hint"] = "Enter the account ID provided by Humm for production environment.",
                ["Plugins.Payments.Humm.Fields.SandboxClientId"] = "Sandbox client ID",
                ["Plugins.Payments.Humm.Fields.SandboxClientId.Hint"] = "Enter the client ID provided by Humm for sandbox environment.",
                ["Plugins.Payments.Humm.Fields.ProductionClientId"] = "Production client ID",
                ["Plugins.Payments.Humm.Fields.ProductionClientId.Hint"] = "Enter the client ID provided by Humm for production environment.",
                ["Plugins.Payments.Humm.Fields.SandboxClientSecret"] = "Sandbox client secret",
                ["Plugins.Payments.Humm.Fields.SandboxClientSecret.Hint"] = "Enter the client secret provided by Humm for sandbox environment.",
                ["Plugins.Payments.Humm.Fields.ProductionClientSecret"] = "Production client secret",
                ["Plugins.Payments.Humm.Fields.ProductionClientSecret.Hint"] = "Enter the client secret provided by Humm for production environment.",
                ["Plugins.Payments.Humm.Fields.SandboxRefreshToken"] = "Sandbox refresh token",
                ["Plugins.Payments.Humm.Fields.SandboxRefreshToken.Hint"] = "Enter the refresh token provided by Humm for sandbox environment.",
                ["Plugins.Payments.Humm.Fields.ProductionRefreshToken"] = "Production refresh token",
                ["Plugins.Payments.Humm.Fields.ProductionRefreshToken.Hint"] = "Enter the refresh token provided by Humm for production environment.",
                ["Plugins.Payments.Humm.Fields.AdditionalFee"] = "Additional fee",
                ["Plugins.Payments.Humm.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Plugins.Payments.Humm.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Plugins.Payments.Humm.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                ["Plugins.Payments.Humm.Fields.AccountId.Required"] = "The account ID is required.",
                ["Plugins.Payments.Humm.Fields.ClientId.Required"] = "The client ID is required.",
                ["Plugins.Payments.Humm.Fields.ClientSecret.Required"] = "The client secret is required.",
                ["Plugins.Payments.Humm.Fields.RefreshToken.Required"] = "The refresh token is required.",
                ["Plugins.Payments.Humm.Fields.AccessToken.Required"] = "The access token is required.",
                ["Plugins.Payments.Humm.Fields.InstanceUrl.Required"] = "The instance URL is required.",
                ["Plugins.Payments.Humm.InvalidPayment"] = "Error when processing the payment transaction. Please try again or contact with store owner.",
                ["Plugins.Payments.Humm.PaymentMethodDescription"] = "Pay by Humm",
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<HummPaymentSettings>();

            var task = await _scheduleTaskService.GetTaskByTypeAsync(HummPaymentDefaults.RefreshPrerequisitesScheduleTask.Type);
            if (task is not null)
                await _scheduleTaskService.DeleteTaskAsync(task);

            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.Humm");

            await base.UninstallAsync();
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        /// <returns>The <see cref="Task"/> containing the payment method description that will be displayed on checkout pages in the public store</returns>
        public Task<string> GetPaymentMethodDescriptionAsync()
        {
            return _localizationService.GetResourceAsync("Plugins.Payments.Humm.PaymentMethodDescription");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => true;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => true;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => true;

        #endregion
    }
}