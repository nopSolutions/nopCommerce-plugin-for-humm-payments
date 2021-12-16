using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Humm.Api.Models;
using Nop.Plugin.Payments.Humm.Services;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.Humm.Controllers
{
    public class HummPaymentController : Controller
    {
        #region Fields

        private readonly HummService _hummService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly INotificationService _notificationService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public HummPaymentController(
            HummService hummService,
            IOrderProcessingService orderProcessingService,
            INotificationService notificationService,
            IPaymentPluginManager paymentPluginManager,
            ILocalizationService localizationService,
            ILogger logger
        )
        {
            _hummService = hummService;
            _orderProcessingService = orderProcessingService;
            _notificationService = notificationService;
            _paymentPluginManager = paymentPluginManager;
            _localizationService = localizationService;
            _logger = logger;
        }

        #endregion

        #region Methods

        [HttpPost]
        public async Task<IActionResult> Confirm([FromBody]ConfirmPaymentRequest request)
        {
            if (request is null)
                return RedirectToAction("Index", "Home");

            if (await _paymentPluginManager.LoadPluginBySystemNameAsync(HummPaymentDefaults.SystemName) is not HummPaymentProcessor processor || !_paymentPluginManager.IsPluginActive(processor))
                return RedirectToAction("Index", "Home");

            var order = await _hummService.GetOrderByExternalIdAsync(request.PartnerTrackingId);
            if (order is null)
                return RedirectToAction("Index", "Home");

            if (request.Success)
            {
                var getTransactionResult = await _hummService.GetTransactionById(request.FlexiTrackingId);
                if (getTransactionResult.Success)
                {
                    var transaction = getTransactionResult.Transaction;
                    if (transaction is null)
                    {
                        await _logger.ErrorAsync(@$"{HummPaymentDefaults.SystemName}: Unsuccessful order confirmation. 
                            The Humm payment transaction with number '{request.FlexiTrackingId}' not found.
                            The order number - {order.CustomOrderNumber}.");
                    }
                    else if (!transaction.Success)
                    {
                        await _logger.ErrorAsync(@$"{HummPaymentDefaults.SystemName}: Unsuccessful order confirmation.
                            Error id - '{transaction.Error?.Id}'.
                            Error code - '{transaction.Error?.Code}'.
                            Error message - '{transaction.Error?.Message}'.");
                    }
                    else if (transaction.Status == PaymentStatus.TrackingIdNotFound)
                    {
                        await _logger.ErrorAsync(@$"{HummPaymentDefaults.SystemName}: Unsuccessful order confirmation. 
                            The Humm payment transaction with number '{request.FlexiTrackingId}' not found.
                            The order number - {order.CustomOrderNumber}.");
                    }
                    else if (transaction.Status == PaymentStatus.PaymentCancelled)
                    {
                        await _logger.ErrorAsync(@$"{HummPaymentDefaults.SystemName}: Unsuccessful order confirmation. 
                            The Humm payment transaction with number '{request.FlexiTrackingId}' was cancelled.
                            The order number - {order.CustomOrderNumber}.");
                    }
                    else
                    {
                        if (_orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            order.CaptureTransactionId = request.FlexiTrackingId;
                            await _orderProcessingService.MarkOrderAsPaidAsync(order);
                        }

                        return RedirectToAction("Completed", "Checkout", new { orderId = order.Id });
                    }
                }
                else
                {
                    await _logger.ErrorAsync(@$"{HummPaymentDefaults.SystemName}: Unsuccessful order confirmation.
                        The order number - {order.CustomOrderNumber}.
                        The payment transaction number - {request.FlexiTrackingId}.
                        {string.Join(Environment.NewLine, getTransactionResult.Errors)}");
                }
            }
            else
            {
                await _logger.ErrorAsync(@$"{HummPaymentDefaults.SystemName}: Unsuccessful order confirmation. 
                    The order number - {order.CustomOrderNumber}.
                    The payment transaction number - {request.FlexiTrackingId}.
                    Error id - '{request.Error?.Id}'.
                    Error code - '{request.Error?.Code}'.
                    Error message - '{request.Error?.Message}'.");
            }

            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync("Plugins.Payments.Humm.InvalidPayment"));

            return RedirectToAction("Details", "Order", new { order.Id });
        }

        public async Task<IActionResult> Cancel(ConfirmPaymentRequest request = null)
        {
            if (await _paymentPluginManager.LoadPluginBySystemNameAsync(HummPaymentDefaults.SystemName) is not HummPaymentProcessor processor || !_paymentPluginManager.IsPluginActive(processor))
                return RedirectToAction("Index", "Home");

            if (request is null)
                return RedirectToAction("Index", "Home");

            var order = await _hummService.GetOrderByExternalIdAsync(request.PartnerTrackingId);
            if (order is null)
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Details", "Order", new { order.Id });
        }

        #endregion
    }
}
