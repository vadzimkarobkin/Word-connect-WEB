using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameAnalyticsSDK;
using UnityEngine;
using UnityEngine.SceneManagement;
using GamePush;
using UnityEngine.Networking;
using WordsToolkit.Scripts.Settings;
using yutokun;

namespace GamePush.Initialization
{
    public class Init : MonoBehaviour
    {
        [SerializeField] private GameSettings _gameSettings;
        private Dictionary<string, string> _data;
        private Dictionary<string, string> _variables = new();

        private void InitSettings()
        {
            _gameSettings.coins = int.Parse(_variables["on_start_coins"]);
            _gameSettings.enableAds = bool.Parse(_variables["enable_ads"]);
            _gameSettings.enableInApps = bool.Parse(_variables["enable_in_apps"]);
            _gameSettings.enableLuckySpin = bool.Parse(_variables["enable_lucky_spin"]);
            _gameSettings.maxFreeLuckySpinCount = int.Parse(_variables["max_free_lucky_spin_count"]);
            _gameSettings.freeLuckySpinRechargeHours = int.Parse(_variables["free_lucky_spin_recharge_hours"]);
            _gameSettings.privacyPolicyUrl = _variables["privacy_policy_url"];
            _gameSettings.continuePrice = int.Parse(_variables["continue_price"]);
            _gameSettings.continueTime = int.Parse(_variables["continue_time"]);
            _gameSettings.gemsForExtraWords = int.Parse(_variables["gems_for_extra_words"]);
            _gameSettings.gemsForGift = int.Parse(_variables["gems_for_gift"]);
            _gameSettings.hammerBoostPrice = int.Parse(_variables["hammer_boost_price"]);
            _gameSettings.hintBoostPrice = int.Parse(_variables["hint_boost_price"]);
            _gameSettings.countOfBoostsToBuy = int.Parse(_variables["count_of_boosts_to_buy"]);
            _gameSettings.boostLevels[0].level = int.Parse(_variables["hammer_booster_button_level"]);
            _gameSettings.boostLevels[1].level = int.Parse(_variables["tip_booster_button_level"]);
            _gameSettings.boostLevels[2].level = int.Parse(_variables["rewarded_tip_level"]);
            
            AdsSettings.InterCooldown = int.Parse(_variables["inter_cooldown"]);
            AdsSettings.RewardedCooldown = int.Parse(_variables["rewarded_cooldown"]);
            AdsSettings.RewardedTipCooldown = int.Parse(_variables["rewardedTip_cooldown"]);
            AdsSettings.InterInsteadRewarded = GP_Variables.GetFloat("interInsteadRewarded");
            AdsSettings.InterFirstShowDelay = int.Parse(_variables["inter_first_show_delay"]);
            AdsSettings.InterMinLevel = int.Parse(_variables["inter_min_level"]);
            AdsSettings.InterPlacement = _variables["inter_placement"];
        }
        
        private async void Start()
        {
            DontDestroyOnLoad(gameObject);
            await GP_Init.Ready;
            
            GP_Variables.Fetch((x) =>
            {
                x.ForEach(y =>
                {
                    _variables.TryAdd(y.key, y.value);
                    Debug.LogError("Key: " + y.key + " Value:" + y.value);
                });
                InitSettings();
            });
            
            Debug.Log("Data imported successfully!");
            
            GameAnalytics.onInitialize += GameAnalyticsOnInitialize;
            GameAnalytics.Initialize();
        }

        private void GameAnalyticsOnInitialize(object sender, bool e)
        {
            GameAnalytics.onInitialize -= GameAnalyticsOnInitialize;
            
            Analytics.GameLoad();
            SceneManager.LoadScene(1);
        }

        private Dictionary<string, string> ParseTable(string text)
        {
            var result = new Dictionary<string, string>();
            var lines = text.Split('\n');

            for (int i = 1; i < lines.Length; i++) // начинаем с 1, пропуская заголовок
            {
                var rawLine = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(rawLine))
                    continue;

                // Разделяем по запятой (т.к. Google Sheets CSV = запятая)
                var parts = rawLine.Split(',');

                if (parts.Length < 2)
                    continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (!string.IsNullOrEmpty(key))
                    result[key] = value;
            }

            return result;
        }
        
        public async UniTask<string> LoadFileAsString(string url)
        {
            var request = UnityWebRequest.Get(url);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await UniTask.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Ошибка загрузки файла: {request.error}");
                return null;
            }

            return request.downloadHandler.text;
        }

        public string GetValue(string key)
        {
            return _data != null && _data.TryGetValue(key, out var value) ? value : null;
        }
        
        public int GetInt(string key)
        {
            string value = GetValue(key);
            if (int.TryParse(value, out int intValue))
                return intValue;

            Debug.LogWarning($"Ключ '{key}' не может быть преобразован в int");
            return 0;
        }
        
        public float GetFloat(string key)
        {
            string value = GetValue(key);
            if (float.TryParse(value.Replace(',', '.'), out float intValue))
                return intValue;

            Debug.LogWarning($"Ключ '{key}' не может быть преобразован в float");
            return 0;
        }

        public bool GetBool(string key)
        {
            string value = GetValue(key);

            if (string.IsNullOrEmpty(value))
                return false;

            value = value.ToLower();

            return value == "true" || value == "1";
        }

    }
}
