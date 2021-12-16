using System;
using Nop.Plugin.Payments.Humm.Api.Models;

namespace Nop.Plugin.Payments.Humm.Api.Client
{
    /// <summary>
    /// Represents a API exception
    /// </summary>
    public class ApiException : Exception
    {
        #region Properties

        /// <summary>
        /// Gets or sets the HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the error response.
        /// </summary>
        public ApiError Error { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="message">The error message.</param>
        public ApiException(int statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="error">The error response.</param>
        public ApiException(int statusCode, string message, ApiError error = null)
            : this(statusCode, message)
        {
            Error = error;
        }

        #endregion
    }
}
