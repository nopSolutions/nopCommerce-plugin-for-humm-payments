using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Payments.Humm.Api.Models;

namespace Nop.Plugin.Payments.Humm.Api.Client
{
    /// <summary>
    /// Provides an abstraction for the HTTP client to interact with the endpoint(s).
    /// </summary>
    public abstract class BaseHttpClient
    {
        #region Fields

        private HttpClient _httpClient;
        private JsonSerializerSettings _defaultWriteSettings;

        #endregion

        #region Properties

        public HummPaymentSettings Settings { get; set; }

        #endregion

        #region Ctor

        public BaseHttpClient(HttpClient httpClient, HummPaymentSettings defaultSettings)
        {
            Settings = defaultSettings;

            httpClient.Timeout = TimeSpan.FromSeconds(HummPaymentDefaults.API.DefaultTimeout);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, HummPaymentDefaults.API.UserAgent);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "*/*");
            _httpClient = httpClient;

            _defaultWriteSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = HummPaymentDefaults.API.DatetimeFormat
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the HTTP request asynchronously.
        /// </summary>
        /// <param name="context">The request context to prepare HTTP request.</param>
        /// <param name="callerName">The caller name.</param>
        /// <returns>The <see cref="Task"/> containing the deserialized body.</returns>
        /// <exception cref="ArgumentNullException">The context is null.</exception>
        /// <exception cref="ApiException">The request failed due to an underlying issue such as 400+ errors, network connectivity, DNS failure, server certificate validation or timeout.</exception>
        protected virtual async Task<TResponse> CallAsync<TResponse>(RequestContext context, [CallerMemberName] string callerName = "")
        {
            var response = await CallAsync(context, callerName);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TResponse>(responseContent);
        }

        /// <summary>
        /// Executes the HTTP request asynchronously.
        /// </summary>
        /// <param name="context">The request context to prepare HTTP request.</param>
        /// <param name="callerName">The caller name.</param>
        /// <returns>The <see cref="Task"/> containing the <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="ArgumentNullException">The context is null.</exception>
        /// <exception cref="ApiException">The request failed due to an underlying issue such as 400+ errors, network connectivity, DNS failure, server certificate validation or timeout.</exception>
        protected virtual async Task<HttpResponseMessage> CallAsync(RequestContext context, [CallerMemberName] string callerName = "")
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (Uri.TryCreate(Settings.InstanceUrl, UriKind.Absolute, out var instanceUri))
                _httpClient.BaseAddress = instanceUri;

            var requestUri = context.Query?.Any() == true
                ? QueryHelpers.AddQueryString(context.Path ?? string.Empty, context.Query)
                : context.Path;

            var request = new HttpRequestMessage(context.Method, requestUri);
            
            request.Headers.Authorization = !string.IsNullOrWhiteSpace(Settings.AccessToken)
                ? new AuthenticationHeaderValue("Bearer", Settings.AccessToken)
                : null;

            foreach (var header in context.Headers)
                request.Headers.Add(header.Key, header.Value);

            if (context.Body != null)
            {
                var content = JsonConvert.SerializeObject(context.Body, _defaultWriteSettings);
                request.Content = new StringContent(content, Encoding.UTF8, MimeTypes.ApplicationJson);
            }

            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (Exception exception)
            {
                throw new ApiException(500, $"Error when calling '{callerName}'. HTTP status code - 500. {exception.Message}");
            }

            var statusCode = (int)response.StatusCode;
            if (statusCode >= 400)
            {
                // throw exception with deserialized error
                var message = $"Error when calling '{callerName}'. HTTP status code - {statusCode}. ";
                var responseContent = await response.Content.ReadAsStringAsync();

                ApiError error = null;
                try
                {
                    error = JsonConvert.DeserializeObject<ApiError>(responseContent);
                }
                catch (JsonSerializationException)
                {
                }

                if (error == null)
                    message = responseContent;
                else
                {
                    message += @$"
                        Error code - '{error.ErrorCode}'.
                        Error description - '{error.ErrorDescription}'.";
                }

                throw new ApiException(statusCode, message, error);
            }

            return response;
        }

        #endregion
    }
}
