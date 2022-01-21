using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Humm.Areas.Admin.Models;
using Nop.Plugin.Payments.Humm.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Humm.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    public class HummConfigurationController : BasePaymentController
    {
        #region Fields

        private readonly HummPaymentSettings _hummPaymentSettings;
        private readonly HummService _hummService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public HummConfigurationController(
            HummPaymentSettings hummPaymentSettings,
            HummService hummService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext
        )
        {
            _hummPaymentSettings = hummPaymentSettings;
            _hummService = hummService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Configures the plugin in admin area.
        /// </summary>
        /// <returns>The view to configure.</returns>
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                IsSandbox = _hummPaymentSettings.IsSandbox,
                SandboxAccountId = _hummPaymentSettings.SandboxAccountId,
                ProductionAccountId = _hummPaymentSettings.ProductionAccountId,
                SandboxClientId = _hummPaymentSettings.SandboxClientId,
                ProductionClientId = _hummPaymentSettings.ProductionClientId,
                SandboxClientSecret = _hummPaymentSettings.SandboxClientSecret,
                ProductionClientSecret = _hummPaymentSettings.ProductionClientSecret,
                SandboxRefreshToken = _hummPaymentSettings.SandboxRefreshToken,
                ProductionRefreshToken = _hummPaymentSettings.ProductionRefreshToken,
                AdditionalFee = _hummPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = _hummPaymentSettings.AdditionalFeePercentage,
            };

            var store = await _storeContext.GetCurrentStoreAsync();
            model.ConfirmPaymentEndpoint = $"{store.Url.TrimEnd('/')}{Url.RouteUrl(HummPaymentDefaults.ConfirmPaymentRouteName)}".ToLowerInvariant();
            model.CancelPaymentEndpoint = $"{store.Url.TrimEnd('/')}{Url.RouteUrl(HummPaymentDefaults.CancelPaymentRouteName)}".ToLowerInvariant();

            return View("~/Plugins/Payments.Humm/Areas/Admin/Views/Configure.cshtml", model);
        }

        /// <summary>
        /// Configures the plugin in admin area.
        /// </summary>
        /// <param name="model">The configuration model.</param>
        /// <returns>The view to configure.</returns>
        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //save settings
            _hummPaymentSettings.IsSandbox = model.IsSandbox;
            _hummPaymentSettings.SandboxAccountId = model.SandboxAccountId;
            _hummPaymentSettings.ProductionAccountId = model.ProductionAccountId;
            _hummPaymentSettings.SandboxClientId = model.SandboxClientId;
            _hummPaymentSettings.ProductionClientId = model.ProductionClientId;
            _hummPaymentSettings.SandboxClientSecret = model.SandboxClientSecret;
            _hummPaymentSettings.ProductionClientSecret = model.ProductionClientSecret;
            _hummPaymentSettings.SandboxRefreshToken = model.SandboxRefreshToken;
            _hummPaymentSettings.ProductionRefreshToken = model.ProductionRefreshToken;
            _hummPaymentSettings.AdditionalFee = model.AdditionalFee;
            _hummPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            var result = await _hummService.GetPrerequisitesAsync(_hummPaymentSettings);
            if (result.Success)
            {
                _hummPaymentSettings.AccessToken = result.AccessToken;
                _hummPaymentSettings.InstanceUrl = result.InstanceUrl;
            }
            else
            {
                _notificationService.ErrorNotification(string.Join(Environment.NewLine, result.Errors));

                return View("~/Plugins/Payments.Humm/Areas/Admin/Views/Configure.cshtml", model);
            }

            await _settingService.SaveSettingAsync(_hummPaymentSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
