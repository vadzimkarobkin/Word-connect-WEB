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
using WordsToolkit.Scripts.GUI.Buttons;
using WordsToolkit.Scripts.Popups.Reward;

namespace WordsToolkit.Scripts.Popups
{
    public class TipRewardPopup : Popup
    {
        [Header("Reward Settings")]
        [SerializeField]
        private RewardedTipReward rewardedTipReward;

        [Header("UI")]
        [SerializeField]
        private CustomButton luckySpinButton;

        [SerializeField]
        private bool closeBeforeOpeningLuckySpin = true;

        protected override void Awake()
        {
            base.Awake();

            if (rewardedTipReward == null)
            {
                rewardedTipReward = FindObjectOfType<RewardedTipReward>();

                if (rewardedTipReward == null)
                {
                    Debug.LogWarning($"{nameof(TipRewardPopup)}: Rewarded tip reward component was not found in the scene.");
                }
            }

            if (luckySpinButton != null)
            {
                luckySpinButton.onClick.AddListener(OnLuckySpinButtonClicked);
            }
            else
            {
                Debug.LogWarning($"{nameof(TipRewardPopup)}: Lucky spin button is not assigned.");
            }
        }

        private void OnDestroy()
        {
            if (luckySpinButton != null)
            {
                luckySpinButton.onClick.RemoveListener(OnLuckySpinButtonClicked);
            }
        }

        /// <summary>
        /// Called from rewarded button handler when the rewarded ad completes successfully.
        /// </summary>
        public void GrantTipReward()
        {
            if (rewardedTipReward == null)
            {
                Debug.LogWarning($"{nameof(TipRewardPopup)}: Rewarded tip reward component is not assigned.");
                return;
            }

            rewardedTipReward.GrantTipAndStartCooldown();
            Close();
        }

        private void OnLuckySpinButtonClicked()
        {
            if (menuManager == null)
            {
                Debug.LogWarning($"{nameof(TipRewardPopup)}: Menu manager is not available.");
                return;
            }

            if (closeBeforeOpeningLuckySpin)
            {
                Close();
            }

            menuManager.ShowPopup<LuckySpin>();
        }
    }
}
