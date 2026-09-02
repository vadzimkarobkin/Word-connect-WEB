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

using UnityEngine;
using UnityEngine.Events;
using VContainer;
using WordsToolkit.Scripts.GUI;
using WordsToolkit.Scripts.GUI.Buttons;
using WordsToolkit.Scripts.Infrastructure.DI;
using WordsToolkit.Scripts.Services;
using WordsToolkit.Scripts.Services.Ads.AdUnits;
using WordsToolkit.Scripts.Settings;

namespace WordsToolkit.Scripts.Popups.Reward
{
    public class RewardedButtonHandler : MonoBehaviour
    {
        [SerializeField] private string placement;
            
        [SerializeField]
        private AdReference adReference;

        [SerializeField]
        private CustomButton rewardedButton;

        [SerializeField]
        private UnityEvent onRewardedAdComplete;

        [SerializeField]
        private UnityEvent onRewardedShow;

        [Inject]
        private IAdsManager adsManager;

        private void Awake()
        {
            rewardedButton.onClick.AddListener(ShowRewardedAd);
        }

        private void ShowRewardedAd()
        {
            // Analytics: Hint Rewarded CTA Click
            var levelManager = FindObjectOfType<WordsToolkit.Scripts.Gameplay.Managers.LevelManager>();
            if (levelManager != null)
            {
                Analytics.HintRewardedCtaClick(levelManager.currentLevel);
            }

            float random = Random.Range(0f, 1f);
            bool interInsteadReward = random < AdsSettings.InterInsteadRewarded;

            string adType = interInsteadReward ? "interstitial" : "rewarded";
                
            Analytics.AdRequest(adType, placement);
                
            if (interInsteadReward)
            {
                Ads.ShowInter(() =>
                {
                    onRewardedShow?.Invoke();
                    Analytics.AdImpression(adType, placement);
                }, (x) =>
                {
                    if (x)
                    {
                        Analytics.AdRewardComplete(adType, placement, "completed");
                        onRewardedAdComplete?.Invoke();
                    }
                    else
                        Analytics.AdRewardComplete(adType, placement, "skipped");
                });
            }
            else
            {
                Ads.ShowRewarded(placement, (x) =>
                {
                    Analytics.AdRewardComplete(adType, placement, "completed");
                    onRewardedShow?.Invoke();
                    onRewardedAdComplete?.Invoke();
                }, () =>
                {
                    Analytics.AdImpression(adType, placement);
                }, (x) =>
                {
                    if (!x)
                        Analytics.AdRewardComplete(adType, placement, "skipped");
                });
            }
        }
    }
}
