// // ©2015 - 2025 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System;
using UnityEngine;

namespace WordsToolkit.Scripts.Services.Ads
{
    public class AdCooldownManager : MonoBehaviour
    {
       private static AdCooldownManager _instance;
        public static AdCooldownManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AdCooldownManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AdCooldownManager");
                        _instance = go.AddComponent<AdCooldownManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private float _lastInterstitialTime = 0f;
        private float _lastRewardedTime = 0f;
        private float _gameStartTime = 0f;
        private bool _hasShownFirstInterstitial = false;
        private bool _hasShownFirstRewarded = false;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                _gameStartTime = Time.realtimeSinceStartup;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Проверяет, можно ли показать interstitial рекламу
        /// </summary>
        public bool CanShowInterstitial()
        {
            float currentTime = Time.realtimeSinceStartup;
            
            // Если это первый показ interstitial, проверяем только минимальное время после старта игры
            if (!_hasShownFirstInterstitial)
            {
                return (currentTime - _gameStartTime) >= AdsSettings.InterFirstShowDelay;
            }
            
            // Для последующих показов проверяем перезарядку
            return (currentTime - _lastInterstitialTime) >= AdsSettings.InterCooldown;
        }

        /// <summary>
        /// Проверяет, можно ли показать rewarded рекламу
        /// </summary>
        public bool CanShowRewarded()
        {
            // Если это первый показ rewarded, всегда разрешаем
            if (!_hasShownFirstRewarded)
            {
                return true;
            }
            
            // Для последующих показов проверяем перезарядку
            float currentTime = Time.realtimeSinceStartup;
            return (currentTime - _lastRewardedTime) >= AdsSettings.RewardedCooldown;
        }

        /// <summary>
        /// Отмечает, что interstitial реклама была показана
        /// </summary>
        public void MarkInterstitialShown()
        {
            _lastInterstitialTime = Time.realtimeSinceStartup;
            _hasShownFirstInterstitial = true;
        }

        /// <summary>
        /// Отмечает, что rewarded реклама была показана
        /// </summary>
        public void MarkRewardedShown()
        {
            _lastRewardedTime = Time.realtimeSinceStartup;
            _hasShownFirstRewarded = true;
        }

        /// <summary>
        /// Возвращает оставшееся время до следующего показа interstitial
        /// </summary>
        public float GetInterstitialCooldownRemaining()
        {
            if (!_hasShownFirstInterstitial)
            {
                float currentTime = Time.realtimeSinceStartup;
                float timeSinceGameStart = currentTime - _gameStartTime;
                return Mathf.Max(0f, AdsSettings.InterFirstShowDelay - timeSinceGameStart);
            }
            
            float currentTime2 = Time.realtimeSinceStartup;
            float timeSinceLastShow = currentTime2 - _lastInterstitialTime;
            return Mathf.Max(0f, AdsSettings.InterCooldown - timeSinceLastShow);
        }

        /// <summary>
        /// Возвращает оставшееся время до следующего показа rewarded
        /// </summary>
        public float GetRewardedCooldownRemaining()
        {
            if (!_hasShownFirstRewarded)
            {
                return 0f; // Первый показ всегда доступен
            }
            
            float currentTime = Time.realtimeSinceStartup;
            float timeSinceLastShow = currentTime - _lastRewardedTime;
            return Mathf.Max(0f, AdsSettings.RewardedCooldown - timeSinceLastShow);
        }

        /// <summary>
        /// Возвращает оставшееся время до первого показа interstitial
        /// </summary>
        public float GetFirstInterstitialDelayRemaining()
        {
            if (_hasShownFirstInterstitial)
                return 0f;
                
            float currentTime = Time.realtimeSinceStartup;
            float timeSinceGameStart = currentTime - _gameStartTime;
            return Mathf.Max(0f, AdsSettings.InterFirstShowDelay - timeSinceGameStart);
        }

        /// <summary>
        /// Сбрасывает все таймеры (для тестирования)
        /// </summary>
        public void ResetCooldowns()
        {
            _lastInterstitialTime = 0f;
            _lastRewardedTime = 0f;
            _gameStartTime = Time.realtimeSinceStartup;
            _hasShownFirstInterstitial = false;
            _hasShownFirstRewarded = false;
        }
    }
}
