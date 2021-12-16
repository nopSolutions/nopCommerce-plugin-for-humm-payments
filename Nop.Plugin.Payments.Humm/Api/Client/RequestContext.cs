using System.Collections.Generic;
using System.Net.Http;

namespace Nop.Plugin.Payments.Humm.Api.Client
{
    /// <summary>
    /// Represents the request context to prepare HTTP request.
    /// </summary>
    public class RequestContext
    {
        #region Properties

        /// <summary>
        /// Gets or sets the relative path to the endpoint.
        /// </summary>
        /// <value>The relative path.</value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the HTTP method.
        /// </summary>
        /// <value>The HTTP method.</value>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the HTTP post body.
        /// </summary>
        /// <value>The HTTP post body.</value>
        public object Body { get; set; }

        /// <summary>
        /// Gets or sets the HTTP request query.
        /// </summary>
        /// <value>The HTTP request query.</value>
        public Dictionary<string, string> Query { get; set; }

        /// <summary>
        /// Gets or sets the HTTP request headers.
        /// </summary>
        /// <value>The HTTP request headers.</value>
        public Dictionary<string, string> Headers { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new instance of the <see cref="RequestContext" /> class.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        public RequestContext(HttpMethod method = null)
        {
            Method = method ?? HttpMethod.Get;
            Query = new Dictionary<string, string>();
            Headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RequestContext" /> class
        /// with relative path to the endpoint and HTTP method.
        /// </summary>
        /// <param name="path">The relative path.</param>
        /// <param name="method">The HTTP method.</param>
        public RequestContext(string path, HttpMethod method = null)
            : this(method)
        {
            Path = path;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a query parameter to the HTTP request.
        /// </summary>
        /// <param name="name">The query parameter name.</param>
        /// <param name="value">The query parameter value.</param>
        public void AddQueryParameter(string name, string value)
        {
            Query[name] = value;
        }

        /// <summary>
        /// Adds a header parameter to the HTTP request.
        /// </summary>
        /// <param name="name">The header parameter name.</param>
        /// <param name="value">The header parameter value.</param>
        public void AddHeader(string name, string value)
        {
            Headers[name] = value;
        }

        #endregion
    }
}
