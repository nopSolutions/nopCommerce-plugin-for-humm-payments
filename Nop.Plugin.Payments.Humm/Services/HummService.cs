using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Humm.Api;
using Nop.Plugin.Payments.Humm.Api.Client;
using Nop.Plugin.Payments.Humm.Api.Models;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.Humm.Services
{
    /// <summary>
    /// Provides an default implementation of the service to manage the Humm integration
    /// </summary>
    public class HummService
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly HummPaymentSettings _hummPaymentSettings;
        private readonly HummApi _hummApi;
        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public HummService(
            CustomerSettings customerSettings,
            HummPaymentSettings hummPaymentSettings,
            HummApi hummApi,
            IAddressService addressService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IGenericAttributeService genericAttributeService,
            IOrderService orderService
        )
        {
            _customerSettings = customerSettings;
            _hummPaymentSettings = hummPaymentSettings;
            _hummApi = hummApi;
            _addressService = addressService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _genericAttributeService = genericAttributeService;
            _orderService = orderService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the value indicating whether to plugin is configured.
        /// </summary>
        /// <returns>The value indicating whether to plugin is configured</returns>
        public virtual bool IsConfigured()
        {
            return ValidateConfiguration(_hummPaymentSettings).Success;
        }

        /// <summary>
        /// Gets the pre-requisites (access token and instance URL) to make an API calls to Humm endpoints.
        /// </summary>
        /// <param name="settings">The Humm plugin settings to get pre-requisites.</param>
        /// <returns>The <see cref="Task"/> containing the result of getting the pre-requisites.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is null</exception>
        public virtual async Task<(bool Success, string AccessToken, string InstanceUrl, IList<string> Errors)> GetPrerequisitesAsync(HummPaymentSettings settings)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            var errors = new List<string>();

            if ((settings.IsSandbox && string.IsNullOrEmpty(settings.SandboxAccountId))
                   || (!settings.IsSandbox && string.IsNullOrEmpty(settings.ProductionAccountId)))
            {
                errors.Add("Plugins.Payments.Humm.Fields.AccountId.Required");
            }

            if ((settings.IsSandbox && string.IsNullOrEmpty(settings.SandboxClientId))
                   || (!settings.IsSandbox && string.IsNullOrEmpty(settings.ProductionClientId)))
            {
                errors.Add("Plugins.Payments.Humm.Fields.ClientId.Required");
            }

            if ((settings.IsSandbox && string.IsNullOrEmpty(settings.SandboxClientSecret))
                   || (!settings.IsSandbox && string.IsNullOrEmpty(settings.ProductionClientSecret)))
            {
                errors.Add("Plugins.Payments.Humm.Fields.ClientSecret.Required");
            }

            if ((settings.IsSandbox && string.IsNullOrEmpty(settings.SandboxRefreshToken))
                   || (!settings.IsSandbox && string.IsNullOrEmpty(settings.ProductionRefreshToken)))
            {
                errors.Add("Plugins.Payments.Humm.Fields.RefreshToken.Required");
            }

            if (errors.Count > 0)
                return (false, null, null, errors);

            _hummApi.Settings = settings;

            try
            {
                var refreshTokenResponse = await _hummApi.GetPrerequisitesAsync();

                return (true, refreshTokenResponse.AccessToken, refreshTokenResponse.InstanceUrl, errors);
            }
            catch (ApiException ex)
            {
                errors.Add(ex.Message);
            }
            finally
            {
                // set settings of current store (if service is use in one scope)
                _hummApi.Settings = _hummPaymentSettings;
            }

            return (false, null, null, errors);
        }

        /// <summary>
        /// Creates the Humm payment transaction by specified order
        /// </summary>
        /// <param name="order">The order to create Humm payment transaction</param>
        /// <returns>The <see cref="Task"/> containing the result of creating the Humm payment transaction.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="order"/> is null</exception>
        public virtual async Task<(bool Success, string RedirectUrl, IList<string> Errors)> CreatePaymentAsync(Order order)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order));

            var errors = new List<string>();

            var validationResult = ValidateConfiguration(_hummPaymentSettings);
            if (!validationResult.Success)
                errors.AddRange(validationResult.Errors);

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            if (customer == null)
                errors.Add($"The customer with id '{order.CustomerId}' not found.");

            var customerShippingAddress = !order.PickupInStore
                ? await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value)
                : null;

            // set available username
            var customerName = string.Empty;
            if (customerShippingAddress != null)
                customerName = customerShippingAddress.FirstName;

            if (string.IsNullOrWhiteSpace(customerName))
            {
                var firstName = await _genericAttributeService
                    .GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                if (!string.IsNullOrWhiteSpace(firstName))
                    customerName = firstName;
                else if (_customerSettings.UsernamesEnabled)
                    customerName = customer.Username;
                else
                    customerName = customer.Email;
            }

            // set available phone number
            var phoneNumber = string.Empty;
            if (customerShippingAddress != null)
                phoneNumber = customerShippingAddress.PhoneNumber;

            if (string.IsNullOrWhiteSpace(phoneNumber) && _customerSettings.PhoneEnabled)
            {
                phoneNumber = await _genericAttributeService
                    .GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute);
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
                errors.Add($"The phone number is required.");

            if (errors.Count > 0)
                return (false, null, errors);

            var createPaymentRequest = new InitiateProcessRequest
            {
                AccountId = _hummPaymentSettings.IsSandbox
                    ? _hummPaymentSettings.SandboxAccountId
                    : _hummPaymentSettings.ProductionAccountId,
                CustomerInfo = new CustomerInfo
                {
                    Name = customerName,
                    Email = customer.Email,
                    MobileNumber = phoneNumber,
                },
                OrderDetails = new OrderDetails
                {
                    OrderId = order.OrderGuid.ToString(),
                    Description = $"№{order.CustomOrderNumber}",
                    Amount = order.OrderTotal * 100,
                    Date = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone)
                }
            };

            try
            {
                var createPaymentResponse = await _hummApi.InitiatePaymentProcessAsync(createPaymentRequest);
                if (createPaymentResponse.Success)
                    return (true, createPaymentResponse.RedirectUrl, errors);
                else
                {
                    errors.Add(@$"
                        Error id - '{createPaymentResponse.Error?.Id}'.
                        Error code - '{createPaymentResponse.Error?.Code}'.
                        Error message - '{createPaymentResponse.Error.Message}'.");
                }
            }
            catch (ApiException ex)
            {
                errors.Add(ex.Message);
            }

            return (false, null, errors);
        }

        /// <summary>
        /// Gets the Humm payment transaction by specified transaction ID
        /// </summary>
        /// <param name="transactionId">The Humm payment transaction ID.</param>
        /// <returns>The <see cref="Task"/> containing the result of getting the Humm payment transaction.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="transactionId"/> is null</exception>
        public virtual async Task<(bool Success, GetPaymentDetailsResponse Transaction, IList<string> Errors)> GetTransactionById(string transactionId)
        {
            if (transactionId is null)
                throw new ArgumentNullException(nameof(transactionId));

            var errors = new List<string>();

            var validationResult = ValidateConfiguration(_hummPaymentSettings);
            if (!validationResult.Success)
                errors.AddRange(validationResult.Errors);

            if (errors.Count > 0)
                return (false, null, errors);

            var getPaymentRequest = new GetPaymentDetailsRequest
            {
                AccountId = _hummPaymentSettings.IsSandbox
                    ? _hummPaymentSettings.SandboxAccountId
                    : _hummPaymentSettings.ProductionAccountId,
                TrackingId = transactionId
            };

            GetPaymentDetailsResponse getPaymentDetailsResponse = null;
            try
            {
                getPaymentDetailsResponse = await _hummApi.GetPaymentDetailsAsync(getPaymentRequest);
            }
            catch (ApiException ex)
            {
                errors.Add(ex.Message);
            }

            return (errors.Count == 0, getPaymentDetailsResponse, errors);
        }

        /// <summary>
        /// Refunds the Humm payment transaction by specified order
        /// </summary>
        /// <param name="order">The order to refund Humm payment transaction</param>
        /// <returns>The <see cref="Task"/> containing the result of refunding the Humm payment transaction.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="order"/> is null</exception>
        public virtual async Task<(bool Success, IList<string> Errors)> RefundOrderAsync(Order order)
        {
            var errors = new List<string>();

            var validationResult = ValidateConfiguration(_hummPaymentSettings);
            if (!validationResult.Success)
                errors.AddRange(validationResult.Errors);

            if (errors.Count > 0)
                return (false, errors);

            var getPaymentRequest = new RefundPaymentRequest
            {
                AccountId = _hummPaymentSettings.IsSandbox
                    ? _hummPaymentSettings.SandboxAccountId
                    : _hummPaymentSettings.ProductionAccountId,
                TrackingId = order.CaptureTransactionId
            };

            try
            {
                await _hummApi.RefundPaymentAsync(getPaymentRequest);
            }
            catch (ApiException ex)
            {
                errors.Add(ex.Message);
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Gets the order by specified external ID.
        /// </summary>
        /// <param name="externalId">The external ID to get the order.</param>
        /// <returns>The <see cref="Task"/> containing the <see cref="Order"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="externalId"/> is null</exception>
        public virtual async Task<Order> GetOrderByExternalIdAsync(string externalId)
        {
            if (externalId is null)
                throw new ArgumentNullException(nameof(externalId));

            var order = Guid.TryParse(externalId, out var orderGuid)
                ? await _orderService.GetOrderByGuidAsync(orderGuid)
                : null;

            return order is null || order.Deleted ? null : order;
        }

        #endregion

        #region Utilities

        private (bool Success, IList<string> Errors) ValidateConfiguration(HummPaymentSettings settings)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            var errors = new List<string>();

            if (string.IsNullOrEmpty(settings.AccessToken))
                errors.Add("Plugins.Payments.Humm.Fields.AccessToken.Required");

            if (!Uri.TryCreate(settings.InstanceUrl, UriKind.RelativeOrAbsolute, out _))
                errors.Add("Plugins.Payments.Humm.Fields.InstanceUrl.Required");

            if (errors.Count > 0)
                errors.Add($"Configure the plugin settings or run the '{HummPaymentDefaults.RefreshPrerequisitesScheduleTask.Name}' schedule task to get new access token and/or instance URL.");

            return (errors.Count == 0, errors);
        }

        #endregion
    }
}
