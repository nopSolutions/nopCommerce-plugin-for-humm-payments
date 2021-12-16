using System;
using System.Linq;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Stores;
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

        private readonly HummService _hummService;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public RefreshPrerequisitesScheduleTask(
            HummService hummService,
            IStoreService storeService,
            ISettingService settingService,
            ILogger logger
        )
        {
            _hummService = hummService;
            _storeService = storeService;
            _settingService = settingService;
            _logger = logger;
        }

        #endregion

        #region Methods

        public async Task ExecuteAsync()
        {
            var stores = await _storeService.GetAllStoresAsync();
            if (stores?.Any() == true)
            {
                foreach (var store in stores)
                {
                    var hummPaymentSettings = await _settingService.LoadSettingAsync<HummPaymentSettings>(store.Id);
                    var result = await _hummService.GetPrerequisitesAsync(hummPaymentSettings);
                    if (result.Success)
                    {
                        hummPaymentSettings.AccessToken = result.AccessToken;
                        hummPaymentSettings.InstanceUrl = result.InstanceUrl;

                        await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.AccessToken, stores.Count > 1, store.Id, false);
                        await _settingService.SaveSettingOverridablePerStoreAsync(hummPaymentSettings, x => x.InstanceUrl, stores.Count > 1, store.Id, false);

                        await _settingService.ClearCacheAsync();
                    }
                    else
                    {
                        await _logger.ErrorAsync(@$"{HummPaymentDefaults.SystemName}: error was occurred while updating the access token 
                            for the '{store.Name}' store in the '{HummPaymentDefaults.RefreshPrerequisitesScheduleTask.Name}' schedule task.
                            {string.Join(Environment.NewLine, result.Errors)}");
                    }
                }
            }
        }

        #endregion
    }
}
