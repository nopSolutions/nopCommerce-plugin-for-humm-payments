using System;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Payments.Humm.Services
{
    /// <summary>
    /// Represents a schedule task to refresh <see cref="HummPaymentSettings.AccessToken"/> and <see cref="HummPaymentSettings.InstanceUrl"/> for store(s).
    /// </summary>
    public class RefreshPrerequisitesScheduleTask : IScheduleTask
    {
        #region Fields

        private readonly HummPaymentSettings _hummPaymentSettings;
        private readonly HummService _hummService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public RefreshPrerequisitesScheduleTask(
            HummPaymentSettings hummPaymentSettings,
            HummService hummService,
            ILogger logger,
            ISettingService settingService
        )
        {
            _hummPaymentSettings = hummPaymentSettings;
            _hummService = hummService;
            _logger = logger;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public async Task ExecuteAsync()
        {
            var result = await _hummService.GetPrerequisitesAsync(_hummPaymentSettings);
            if (result.Success)
            {
                _hummPaymentSettings.AccessToken = result.AccessToken;
                _hummPaymentSettings.InstanceUrl = result.InstanceUrl;

                await _settingService.SaveSettingAsync(_hummPaymentSettings);
            }
            else
            {
                await _logger.ErrorAsync(@$"{HummPaymentDefaults.SystemName}: error was occurred while updating the access token 
                    in the '{HummPaymentDefaults.RefreshPrerequisitesScheduleTask.Name}' schedule task.
                    {string.Join(Environment.NewLine, result.Errors)}");
            }
        }

        #endregion
    }
}
