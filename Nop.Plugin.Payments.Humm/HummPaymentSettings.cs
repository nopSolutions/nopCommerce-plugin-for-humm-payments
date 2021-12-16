using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Humm
{
    /// <summary>
    /// Represents the settings of Humm payment plugin
    /// </summary>
    public class HummPaymentSettings : ISettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to sandbox environment is active
        /// </summary>
        public bool IsSandbox { get; set; }

        /// <summary>
        /// Gets or sets the sandbox account ID assigned by humm.
        /// </summary>
        public string SandboxAccountId { get; set; }

        /// <summary>
        /// Gets or sets the production account ID assigned by humm.
        /// </summary>
        public string ProductionAccountId { get; set; }

        /// <summary>
        /// Gets or sets the sandbox client ID
        /// </summary>
        public string SandboxClientId { get; set; }

        /// <summary>
        /// Gets or sets the production client ID
        /// </summary>
        public string ProductionClientId { get; set; }

        /// <summary>
        /// Gets or sets the sandbox client secret
        /// </summary>
        public string SandboxClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the production client secret
        /// </summary>
        public string ProductionClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the sandbox refresh token
        /// </summary>
        public string SandboxRefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the production refresh token
        /// </summary>
        public string ProductionRefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the access token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the instance url
        /// </summary>
        public string InstanceUrl { get; set; }

        /// <summary>
        /// Gets or sets an additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }

        #endregion
    }
}
