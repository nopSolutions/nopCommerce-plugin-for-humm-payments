using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Humm.Api.Models
{
    /// <summary>
    /// Represents a model for the pre-requisites API response
    /// </summary>
    public class GetPrerequisitesResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the access token
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    
        /// <summary>
        /// Gets or sets the instance url
        /// </summary>
        [JsonProperty("instance_url")]
        public string InstanceUrl { get; set; }

        #endregion
    }
}
