using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Humm.Models
{
    /// <summary>
    /// Represents a plugin configuration model.
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to sandbox environment is active
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Humm.Fields.IsSandbox")]
        public bool IsSandbox { get; set; }

        /// <summary>
        /// Gets or sets the sandbox account ID assigned by humm.
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Humm.Fields.SandboxAccountId")]
        public string SandboxAccountId { get; set; }

        /// <summary>
        /// Gets or sets the production account ID assigned by humm.
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Humm.Fields.ProductionAccountId")]
        public string ProductionAccountId { get; set; }

        /// <summary>
        /// Gets or sets the sandbox client ID
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Humm.Fields.SandboxClientId")]
        public string SandboxClientId { get; set; }

        /// <summary>
        /// Gets or sets the production client ID
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Humm.Fields.ProductionClientId")]
        public string ProductionClientId { get; set; }

        /// <summary>
        /// Gets or sets the sandbox client secret
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Humm.Fields.SandboxClientSecret")]
        public string SandboxClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the production client secret
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Humm.Fields.ProductionClientSecret")]
        public string ProductionClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the sandbox refresh token
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Humm.Fields.SandboxRefreshToken")]
        public string SandboxRefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the production refresh token
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Humm.Fields.ProductionRefreshToken")]
        public string ProductionRefreshToken { get; set; }

        /// <summary>
        /// Gets or sets an additional fee
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Humm.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Humm.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }

        #endregion
    }
}