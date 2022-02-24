using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Humm.Models;
using Nop.Plugin.Payments.Humm.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Humm.Controllers
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

        #endregion

        #region Ctor

        public HummConfigurationController(HummPaymentSettings hummPaymentSettings,
            HummService hummService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _hummPaymentSettings = hummPaymentSettings;
            _hummService = hummService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

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

            return View("~/Plugins/Payments.Humm/Views/Configure.cshtml", model);
        }

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

                return View("~/Plugins/Payments.Humm/Views/Configure.cshtml", model);
            }

            await _settingService.SaveSettingAsync(_hummPaymentSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}