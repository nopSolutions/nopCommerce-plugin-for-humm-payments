using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Humm.Api.Models;
using Nop.Plugin.Payments.Humm.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Humm.Controllers
{
    public class HummPaymentController : BasePaymentController
    {
        #region Fields

        private readonly HummService _hummService;
        private readonly HummPaymentSettings _hummPaymentSettings;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;

        #endregion

        #region Ctor

        public HummPaymentController(HummService hummService,
            HummPaymentSettings hummPaymentSettings,
            ICheckoutModelFactory checkoutModelFactory,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            IOrderModelFactory orderModelFactory,
            IOrderProcessingService orderProcessingService,
            IPaymentPluginManager paymentPluginManager,
            IWorkContext workContext,
            OrderSettings orderSettings)
        {
            _hummService = hummService;
            _hummPaymentSettings = hummPaymentSettings;
            _checkoutModelFactory = checkoutModelFactory;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _orderModelFactory = orderModelFactory;
            _orderProcessingService = orderProcessingService;
            _paymentPluginManager = paymentPluginManager;
            _workContext = workContext;
            _orderSettings = orderSettings;
        }

        #endregion

        #region Methods

        [HttpPost]
        public async Task<IActionResult> CheckoutCompleted(Guid token, [FromForm] ConfirmPaymentRequest request)
        {
            try
            {
                var plugin = await _paymentPluginManager.LoadPluginBySystemNameAsync(HummPaymentDefaults.SystemName);
                if (!_paymentPluginManager.IsPluginActive(plugin))
                    throw new NopException("Order confirmation. Plugin isn't active anymore");

                var customer = await _customerService.GetCustomerByGuidAsync(token);
                if (customer is null || !customer.Active || customer.Deleted)
                    throw new NopException($"Order cancellation. No customer found with GUID '{token}'");

                var trackingId = !string.IsNullOrEmpty(request?.PartnerTrackingId) ? request.PartnerTrackingId : request?.FlexiTrackingId;
                if (string.IsNullOrEmpty(trackingId))
                    throw new NopException("Order confirmation. Parameters are in an incorrect format");

                var order = await _hummService.GetOrderByExternalIdAsync(trackingId);
                if (order is null)
                    throw new NopException($"Order confirmation. Order with the payment transaction number '{trackingId}' is not found");

                if (order.CustomerId != customer.Id)
                    throw new NopException("Order cancellation. The current customer doesn't match the customer who placed the order");

                await _workContext.SetCurrentCustomerAsync(customer);

                var (success, transaction, errors) = await _hummService.GetTransactionById(trackingId);
                if (!success || transaction is null || !transaction.Success || !transaction.Status.HasValue)
                {
                    throw new CheckoutException(trackingId, $"Order confirmation. Payment transaction '{trackingId}' " +
                        $"(order '{order.CustomOrderNumber}') failed. {string.Join(Environment.NewLine, errors)}");
                }

                switch (transaction.Status)
                {
                    case PaymentStatus.NotFound:
                    case PaymentStatus.NotStarted:
                    case PaymentStatus.InProgress:
                    case PaymentStatus.Cancelled:
                        throw new CheckoutException(trackingId, $"Order confirmation. Payment transaction '{trackingId}' " +
                            $"(order '{order.CustomOrderNumber}') { CommonHelper.ConvertEnum(transaction.Status.ToString())}");

                    case PaymentStatus.Completed:
                        if (request.PaymentAmount.HasValue && request.PaymentAmount.Value != order.OrderTotal)
                        {
                            throw new CheckoutException(trackingId, $"Order confirmation. Order total doesn't match the amount paid. " +
                                $"Payment transaction '{trackingId}' (order '{order.CustomOrderNumber}'). " +
                                $"Order total is {order.OrderTotal}, but was paid {request.PaymentAmount}");
                        }

                        if (_orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            order.CaptureTransactionId = trackingId;
                            await _orderProcessingService.MarkOrderAsPaidAsync(order);
                        }

                        await _genericAttributeService.SaveAttributeAsync<string>(order, HummPaymentDefaults.TrackingIdAttribute, null);

                        if (_orderSettings.DisableOrderCompletedPage)
                            return View("~/Views/Order/Details.cshtml", await _orderModelFactory.PrepareOrderDetailsModelAsync(order));

                        return View("~/Views/Checkout/Completed.cshtml", await _checkoutModelFactory.PrepareCheckoutCompletedModelAsync(order));

                    case PaymentStatus.Refunded:
                    case PaymentStatus.RefundInProgress:
                    case PaymentStatus.PartiallyRefunded:
                    case PaymentStatus.RefundRequested:
                        break;
                }

                return RedirectToRoute("Homepage");
            }
            catch (Exception exception)
            {
                if (_hummPaymentSettings.LogIpnErrors)
                {
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    await _logger.ErrorAsync($"{HummPaymentDefaults.SystemName}: {exception.Message}", exception, customer);
                }

                if (exception is CheckoutException checkoutException)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Payments.Humm.InvalidPayment"));
                    var order = await _hummService.GetOrderByExternalIdAsync(checkoutException.TrackingId);
                    if (order is null)
                        return RedirectToRoute("Homepage");

                    return View("~/Views/Order/Details.cshtml", await _orderModelFactory.PrepareOrderDetailsModelAsync(order));
                }

                return RedirectToRoute("Homepage");
            }
        }

        public async Task<IActionResult> CheckoutCompleted(Guid token, CancelRequest request = null)
        {
            try
            {
                var plugin = await _paymentPluginManager.LoadPluginBySystemNameAsync(HummPaymentDefaults.SystemName);
                if (!_paymentPluginManager.IsPluginActive(plugin))
                    throw new NopException("Order cancellation. Plugin isn't active anymore");

                var customer = await _customerService.GetCustomerByGuidAsync(token);
                if (customer is null || !customer.Active || customer.Deleted)
                    throw new NopException($"Order cancellation. No customer found with GUID '{token}'");

                if (string.IsNullOrEmpty(request?.OrderId))
                    throw new NopException("Order cancellation. Parameters are in an incorrect format");

                var order = await _hummService.GetOrderByExternalIdAsync(request.OrderId);
                if (order is null)
                    throw new NopException($"Order cancellation. Order with the payment transaction number '{request.OrderId}' is not found");

                if (order.CustomerId != customer.Id)
                    throw new NopException("Order cancellation. The current customer doesn't match the customer who placed the order");

                await _workContext.SetCurrentCustomerAsync(customer);

                if (_orderProcessingService.CanCancelOrder(order))
                    await _orderProcessingService.CancelOrderAsync(order, false);

                return View("~/Views/Order/Details.cshtml", await _orderModelFactory.PrepareOrderDetailsModelAsync(order));
            }
            catch (Exception exception)
            {
                if (_hummPaymentSettings.LogIpnErrors)
                {
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    await _logger.ErrorAsync($"{HummPaymentDefaults.SystemName}: {exception.Message}", exception, customer);
                }

                return RedirectToRoute("Homepage");
            }
        }

        #endregion
    }
}