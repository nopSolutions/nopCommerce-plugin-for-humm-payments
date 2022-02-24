using FluentValidation;
using Nop.Plugin.Payments.Humm.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.Humm.Validators
{
    /// <summary>
    /// Represents a validator for <see cref="ConfigurationModel"/>
    /// </summary>
    public class ConfigurationModelValidator : BaseNopValidator<ConfigurationModel>
    {
        #region Ctor

        public ConfigurationModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.SandboxAccountId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Humm.Fields.AccountId.Required"))
                .When(model => model.IsSandbox);

            RuleFor(model => model.ProductionAccountId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Humm.Fields.AccountId.Required"))
                .When(model => !model.IsSandbox);

            RuleFor(model => model.SandboxClientId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Humm.Fields.ClientId.Required"))
                .When(model => model.IsSandbox);

            RuleFor(model => model.ProductionClientId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Humm.Fields.ClientId.Required"))
                .When(model => !model.IsSandbox);

            RuleFor(model => model.SandboxClientSecret)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Humm.Fields.ClientSecret.Required"))
                .When(model => model.IsSandbox);

            RuleFor(model => model.ProductionClientSecret)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Humm.Fields.ClientSecret.Required"))
                .When(model => !model.IsSandbox);

            RuleFor(model => model.SandboxRefreshToken)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Humm.Fields.RefreshToken.Required"))
                .When(model => model.IsSandbox);

            RuleFor(model => model.ProductionRefreshToken)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Humm.Fields.RefreshToken.Required"))
                .When(model => !model.IsSandbox);
        }

        #endregion
    }
}