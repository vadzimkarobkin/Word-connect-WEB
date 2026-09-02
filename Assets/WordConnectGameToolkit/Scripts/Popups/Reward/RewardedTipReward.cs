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
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WordsToolkit.Scripts.Data;
using WordsToolkit.Scripts.GUI.Buttons;

namespace WordsToolkit.Scripts.Popups.Reward
{
    public class RewardedTipReward : MonoBehaviour
    {
        [SerializeField]
        private ResourceItem tipResource;

        [SerializeField]
        private CustomButton rewardedButton;

        [SerializeField]
        private float cooldownDuration = 60f;

        [SerializeField]
        private GameObject iconContainer;
        [SerializeField]
        private GameObject TextContainer;

        [SerializeField]
        private TMP_Text cooldownLabel;

        [SerializeField]
        private Color inactiveButtonColor = new Color(1f, 1f, 1f, 0.4f);

        private Coroutine cooldownRoutine;
        private float cooldownEndTime = float.NegativeInfinity;
        private Image rewardedButtonImage;
        private Color activeButtonColor;
        private bool activeColorCached;

        public void GrantTipAndStartCooldown()
        {
            AddTipReward();
            StartCooldown();
        }

        private void OnEnable()
        {
            cooldownDuration = AdsSettings.RewardedTipCooldown;
            CacheButtonVisualState();

            if (IsCooldownActive())
            {
                if (cooldownRoutine == null)
                {
                    cooldownRoutine = StartCoroutine(CooldownRoutine());
                }
            }
            else if (cooldownRoutine == null)
            {
                EnsureButtonInteractable();
            }
        }

        private void AddTipReward()
        {
            if (tipResource == null)
            {
                Debug.LogWarning("Tip resource is not assigned for RewardedTipReward.");
                return;
            }

            tipResource.Add(1);
            
            // Analytics: Coins Earned (if it's a coin resource)
            var levelManager = FindObjectOfType<WordsToolkit.Scripts.Gameplay.Managers.LevelManager>();
            if (levelManager != null && tipResource.name.ToLower().Contains("coin"))
            {
                Analytics.CoinsEarned(levelManager.currentLevel, 1);
            }
        }

        private void StartCooldown()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (rewardedButton == null)
            {
                Debug.LogWarning("Rewarded button is not assigned for RewardedTipReward.");
                return;
            }

            CacheButtonVisualState();

            if (cooldownRoutine != null)
            {
                StopCoroutine(cooldownRoutine);
            }

            cooldownEndTime = Time.realtimeSinceStartup + cooldownDuration;
            cooldownRoutine = StartCoroutine(CooldownRoutine());
        }

        private IEnumerator CooldownRoutine()
        {
            rewardedButton.interactable = false;
            UpdateCooldownVisuals();

            while (IsCooldownActive())
            {
                UpdateCooldownVisuals();
                yield return null;
            }

            cooldownRoutine = null;
            EnsureButtonInteractable();
        }

        private void EnsureButtonInteractable()
        {
            if (rewardedButton != null)
            {
                rewardedButton.interactable = true;
            }

            RestoreActiveVisuals();
        }

        private void CacheButtonVisualState()
        {
            if (rewardedButton == null || rewardedButtonImage != null)
            {
                return;
            }

            rewardedButtonImage = rewardedButton.image;
            if (rewardedButtonImage != null)
            {
                activeButtonColor = rewardedButtonImage.color;
                activeColorCached = true;
            }
        }

        private bool IsCooldownActive()
        {
            return Time.realtimeSinceStartup < cooldownEndTime;
        }

        private void UpdateCooldownVisuals()
        {
            if (iconContainer != null)
            {
                iconContainer.SetActive(false);
            }

            if (cooldownLabel != null)
            {
                TextContainer.SetActive(true);
                cooldownLabel.gameObject.SetActive(true);
                var remainingSeconds = Mathf.Max(0f, cooldownEndTime - Time.realtimeSinceStartup);
                cooldownLabel.text = FormatCooldown(remainingSeconds);
            }

            if (rewardedButtonImage != null)
            {
                rewardedButtonImage.color = inactiveButtonColor;
            }
        }

        private void RestoreActiveVisuals()
        {
            if (iconContainer != null)
            {
                iconContainer.SetActive(true);
            }

            if (cooldownLabel != null)
            {
                cooldownLabel.gameObject.SetActive(false);
                cooldownLabel.text = string.Empty;
                TextContainer.SetActive(false);
            }

            if (rewardedButtonImage != null && activeColorCached)
            {
                rewardedButtonImage.color = activeButtonColor;
            }
        }

        private static string FormatCooldown(float remainingSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(Mathf.CeilToInt(remainingSeconds));
            return timeSpan.TotalHours >= 1
                ? timeSpan.ToString(@"hh\:mm\:ss")
                : timeSpan.ToString(@"mm\:ss");
        }
    }
}
