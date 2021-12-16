using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Humm.Api;
using Nop.Plugin.Payments.Humm.Api.Client;
using Nop.Plugin.Payments.Humm.Api.Models;
using Nop.Plugin.Payments.Humm.Areas.Admin.Models;
using Nop.Plugin.Payments.Humm.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Humm.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    public class HummConfigurationController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly HummService _hummService;

        #endregion

        #region Ctor

        public HummConfigurationController(
            IPermissionService permissionService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IStoreService storeService,
            HummService hummService
        )
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _storeService = storeService;
            _hummService = hummService;
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

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var hummPaymentSettings = await _settingService.LoadSettingAsync<HummPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                IsSandbox = hummPaymentSettings.IsSandbox,
                SandboxAccountId = hummPaymentSettings.SandboxAccountId,
                ProductionAccountId = hummPaymentSettings.ProductionAccountId,
                SandboxClientId = hummPaymentSettings.SandboxClientId,
                ProductionClientId = hummPaymentSettings.ProductionClientId,
                SandboxClientSecret = hummPaymentSettings.SandboxClientSecret,
                ProductionClientSecret = hummPaymentSettings.ProductionClientSecret,
                SandboxRefreshToken = hummPaymentSettings.SandboxRefreshToken,
                ProductionRefreshToken = hummPaymentSettings.ProductionRefreshToken,
                AdditionalFee = hummPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = hummPaymentSettings.AdditionalFeePercentage,
            };

            var store = storeScope > 0
                ? await _storeService.GetStoreByIdAsync(storeScope)
                : await _storeContext.GetCurrentStoreAsync();
            model.ConfirmPaymentEndpoint = $"{store.Url.TrimEnd('/')}{Url.RouteUrl(HummPaymentDefaults.ConfirmPaymentRouteName)}".ToLowerInvariant();
            model.CancelPaymentEndpoint = $"{store.Url.TrimEnd('/')}{Url.RouteUrl(HummPaymentDefaults.CancelPaymentRouteName)}".ToLowerInvariant();

            if (storeScope > 0)
            {
                model.IsSandbox_OverrideForStore = await _settingService.SettingExistsAsync(hummPaymentSettings, x => x.IsSandbox, storeScope);
                model.SandboxAccountId_OverrideForStore = await _settingService.SettingExistsAsync(hummPaymentSettings, x => x.SandboxAccountId, storeScope);
                model.ProductionAccountId_OverrideForStore = await _settingService.SettingExistsAsync(hummPaymentSettings, x => x.ProductionAccountId, storeScope);
                model.SandboxClientId_OverrideForStore = await _settingService.SettingExistsAsync(hummPaymentSettings, x => x.SandboxClientId, storeScope);
                model.ProductionClientId_OverrideForStore = await _settingService.SettingExistsAsync(hummPaymentSettings, x => x.ProductionClientId, storeScope);
                model.SandboxClientSecret_OverrideForStore = await _settingService.SettingExistsAsync(hummPaymentSettings, x => x.SandboxClientSecret, storeScope);
                model.ProductionClientSecret_OverrideForStore = await _settingService.SettingExistsAsync(hummPaymentSettings, x => x.ProductionClientSecret, storeScope);
                model.SandboxRefreshToken_OverrideForStore = await _settingService.SettingExistsAsync(hummPaymentSettings, x => x.SandboxRefreshToken, storeScope);
                model.ProductionRefreshToken_OverrideForStore = await _settingService.SettingExistsAsync(hummPaymentSettings, x => x.ProductionRefreshToken, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(hummPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(hummPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

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

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var hummPaymentSettings = await _settingService.LoadSettingAsync<HummPaymentSettings>(storeScope);
             
            //save settings
            hummPaymentSettings.IsSandbox = model.IsSandbox;
            hummPaymentSettings.SandboxAccountId = model.SandboxAccountId;
            hummPaymentSettings.ProductionAccountId = model.ProductionAccountId;
            hummPaymentSettings.SandboxClientId = model.SandboxClientId;
            hummPaymentSettings.ProductionClientId = model.ProductionClientId;
            hummPaymentSettings.SandboxClientSecret = model.SandboxClientSecret;
            hummPaymentSettings.ProductionClientSecret = model.ProductionClientSecret;
            hummPaymentSettings.SandboxRefreshToken = model.SandboxRefreshToken;
            hummPaymentSettings.ProductionRefreshToken = model.ProductionRefreshToken;
            hummPaymentSettings.AdditionalFee = model.AdditionalFee;
            hummPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            var result = await _hummService.GetPrerequisitesAsync(hummPaymentSettings);
            if (result.Success)
            {
                hummPaymentSettings.AccessToken = result.AccessToken;
                hummPaymentSettings.InstanceUrl = result.InstanceUrl;
            }
            else
            {
                _notificationService.ErrorNotification(string.Join(Environment.NewLine, result.Errors));

                return View("~/Plugins/Payments.Humm/Areas/Admin/Views/Configure.cshtml", model);
            }

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.IsSandbox, model.IsSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.SandboxAccountId, model.SandboxAccountId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.ProductionAccountId, model.ProductionAccountId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.SandboxClientId, model.SandboxClientId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.ProductionClientId, model.ProductionClientId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.SandboxClientSecret, model.SandboxClientSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.ProductionClientSecret, model.ProductionClientSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.SandboxRefreshToken, model.SandboxRefreshToken_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.ProductionRefreshToken, model.ProductionRefreshToken_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.AccessToken, storeScope > 0, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.InstanceUrl, storeScope > 0, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
