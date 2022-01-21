using System;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Plugin.Payments.Humm.Api.Client;
using Nop.Plugin.Payments.Humm.Api.Models;

namespace Nop.Plugin.Payments.Humm.Api
{
    /// <summary>
    /// Provides an default implementation of the HTTP client to interact with the Humm endpoints
    /// </summary>
    public class HummApi : BaseHttpClient
    {
        #region Ctor

        public HummApi(HttpClient httpClient, HummPaymentSettings defaultSettings)
            : base(httpClient, defaultSettings)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the pre-requisites with current <see cref="BaseHttpClient.Settings"/>.
        /// </summary>
        /// <returns>The <see cref="Task"/> containing the <see cref="GetPrerequisitesResponse"/>.</returns>
        /// <exception cref="ApiException">The request failed due to an underlying issue such as 400+ errors, network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public virtual Task<GetPrerequisitesResponse> GetPrerequisitesAsync()
        {
            var authorizeEndpointUri = Settings.IsSandbox
                ? HummPaymentDefaults.API.Endpoints.AuthorizeSandbox
                : HummPaymentDefaults.API.Endpoints.AuthorizeProduction;
            var refreshToken = Settings.IsSandbox
                ? Settings.SandboxRefreshToken
                : Settings.ProductionRefreshToken;
            var clientId = Settings.IsSandbox
                ? Settings.SandboxClientId
                : Settings.ProductionClientId;
            var clientSecret = Settings.IsSandbox
                ? Settings.SandboxClientSecret
                : Settings.ProductionClientSecret;
            var requestContext = new RequestContext(authorizeEndpointUri, HttpMethod.Post);

            requestContext.AddQueryParameter("grant_type", "refresh_token");
            requestContext.AddQueryParameter("refresh_token", refreshToken);
            requestContext.AddQueryParameter("client_id", clientId);
            requestContext.AddQueryParameter("client_secret", clientSecret);

            return CallAsync<GetPrerequisitesResponse>(requestContext);
        }

        /// <summary>
        /// Starts the payment process
        /// </summary>
        /// <param name="request">The request to initiate payment process</param>
        /// <returns>The <see cref="Task"/> containing the <see cref="InitiateProcessResponse"/>.</returns>
        /// <exception cref="ArgumentNullException">The request is null.</exception>
        /// <exception cref="ApiException">The request failed due to an underlying issue such as 400+ errors, network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public virtual Task<InitiateProcessResponse> InitiatePaymentProcessAsync(InitiateProcessRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var requestContext = new RequestContext
            {
                Path = HummPaymentDefaults.API.Endpoints.InitiatePaymentProcess,
                Method = HttpMethod.Post,
                Body = request
            };
            return CallAsync<InitiateProcessResponse>(requestContext);
        }

        /// <summary>
        /// Gets the payment details
        /// </summary>
        /// <param name="request">The request to get payment details</param>
        /// <returns>The <see cref="Task"/> containing the <see cref="GetPaymentDetailsResponse"/>.</returns>
        /// <exception cref="ArgumentNullException">The request is null.</exception>
        /// <exception cref="ApiException">The request failed due to an underlying issue such as 400+ errors, network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public virtual Task<GetPaymentDetailsResponse> GetPaymentDetailsAsync(GetPaymentDetailsRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var requestContext = new RequestContext
            {
                Path = HummPaymentDefaults.API.Endpoints.PaymentDetails,
                Method = HttpMethod.Post,
                Body = request
            };
            return CallAsync<GetPaymentDetailsResponse>(requestContext);
        }

        /// <summary>
        /// Refunds the payment
        /// </summary>
        /// <param name="request">The request to refund payment</param>
        /// <returns>The <see cref="Task"/> containing the <see cref="RefundPaymentResponse"/>.</returns>
        /// <exception cref="ArgumentNullException">The request is null.</exception>
        /// <exception cref="ApiException">The request failed due to an underlying issue such as 400+ errors, network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public virtual Task<RefundPaymentResponse> RefundPaymentAsync(RefundPaymentRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var requestContext = new RequestContext
            {
                Path = HummPaymentDefaults.API.Endpoints.RefundPayment,
                Method = HttpMethod.Post,
                Body = request
            };
            return CallAsync<RefundPaymentResponse>(requestContext);
        }

        #endregion
    }
}
