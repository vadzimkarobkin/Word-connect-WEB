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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using WordsToolkit.Scripts.Audio;
using WordsToolkit.Scripts.Data;
using WordsToolkit.Scripts.GUI;
using WordsToolkit.Scripts.GUI.Buttons;
using WordsToolkit.Scripts.GUI.Labels;
using WordsToolkit.Scripts.Popups.Reward;
using WordsToolkit.Scripts.Settings;
using WordsToolkit.Scripts.System;
using Random = UnityEngine.Random;

namespace WordsToolkit.Scripts.Popups
{
    public class LuckySpin : PopupWithCurrencyLabel
    {
        public float velocity;
        public float stoptime;

        [SerializeField]
        private GameObject spin;

        [SerializeField]
        private List<Image> lights = new();

        public static event Action<int> FreeSpinCountChanged;

        private static int lastBroadcastFreeSpinCount = -1;

        public CustomButton freeSpinButton;
        public CustomButton rewardedAdButton;
        public RewardSettingSpin[] spinRewards;
        public List<RewardVisual> rewards = new();

        [SerializeField]
        private TMP_Text freeSpinCountLabel;

        [SerializeField]
        private TMP_Text nextFreeSpinTimerLabel;
        private string freeSpinCountLabelBaseText;
        private string nextFreeSpinTimerLabelBaseText;
        private SpinSettings spinSettings;
        private Rigidbody2D rb;
        private bool isSpinning;
        private int previousRotationMarker;
        private const string LastFreeSpinTimeKey = "LastFreeSpinTime";
        private const string FreeSpinCountKey = "LuckySpinFreeSpinCount";
        private int availableFreeSpins;
        private float freeSpinUpdateTimer;

        [SerializeField]
        private float stopDampingMultiplier = 100f;

        [SerializeField]
        private float stopAngularVelocityThreshold = 0.1f;

        private float initialAngularDamping;
        private bool angularDampingInitialized;

        [SerializeField]
        private float minVelocityMultiplier = 0.5f;

        [SerializeField]
        private float maxVelocityMultiplier = 2.5f;

        [SerializeField]
        private float additionalRandomFactor = 0.2f;
        [Inject]
        private SpinSettings luckySpinSettings;

        [SerializeField]
        private AudioClip luckySpin;

        [SerializeField]
        private AudioClip applause;

        private void OnEnable()
        {
            rb = spin.GetComponent<Rigidbody2D>();
            CacheInitialAngularDamping();
            CacheLabelBaseText(freeSpinCountLabel, ref freeSpinCountLabelBaseText);
            CacheLabelBaseText(nextFreeSpinTimerLabel, ref nextFreeSpinTimerLabelBaseText);
            freeSpinButton.onClick.AddListener(FreeSpin);

            UpdateButtonVisibility();

            spinSettings = luckySpinSettings;
            DefineRewards(spinSettings.rewards);
            StartCoroutine(SwitchLightsAlpha());
        }

        private static void CacheLabelBaseText(TMP_Text label, ref string cachedText)
        {
            if (label == null || !string.IsNullOrEmpty(cachedText))
            {
                return;
            }

            cachedText = label.text;
        }

        private static string CombineBaseTextWithValue(string baseText, string value)
        {
            if (string.IsNullOrEmpty(baseText))
            {
                return value;
            }

            if (string.IsNullOrEmpty(value))
            {
                return baseText;
            }

            return baseText.EndsWith(" ", StringComparison.Ordinal)
                ? string.Concat(baseText, value)
                : string.Concat(baseText, " ", value);
        }

        private static void NotifyFreeSpinCountChanged(int count)
        {
            if (lastBroadcastFreeSpinCount == count)
            {
                return;
            }

            lastBroadcastFreeSpinCount = count;
            FreeSpinCountChanged?.Invoke(count);
        }

        private static int NotifyAndReturn(int count)
        {
            NotifyFreeSpinCountChanged(count);
            return count;
        }

        private void RefreshFreeSpinStatusLabels()
        {
            RefreshAvailableFreeSpins();
            UpdateFreeSpinStatusLabels();
        }

        public override void AfterShowAnimation()
        {
            base.AfterShowAnimation();
            RefreshFreeSpinStatusLabels();
        }

        private void OnDisable()
        {
            freeSpinButton.onClick.RemoveListener(FreeSpin);
        }

        private void UpdateButtonVisibility()
        {
            RefreshFreeSpinStatusLabels();
            var hasFreeSpins = availableFreeSpins > 0;
            freeSpinButton.gameObject.SetActive(hasFreeSpins);
            rewardedAdButton.gameObject.SetActive(!hasFreeSpins);
            freeSpinButton.interactable = hasFreeSpins && !isSpinning;
        }

        private void SetButtonsVisibility(bool visible)
        {
            freeSpinButton.gameObject.SetActive(visible);
            rewardedAdButton.gameObject.SetActive(visible);
        }

        private int RefreshAvailableFreeSpins()
        {
            availableFreeSpins = GetAvailableFreeSpins(gameSettings);
            return availableFreeSpins;
        }

        public static int GetAvailableFreeSpins(GameSettings settings)
        {
            if (settings == null)
            {
                return NotifyAndReturn(0);
            }

            var maxSpins = Mathf.Max(0, settings.maxFreeLuckySpinCount);
            var storedSpins = PlayerPrefs.GetInt(FreeSpinCountKey, maxSpins);
            var availableSpins = Mathf.Clamp(storedSpins, 0, maxSpins);
            if (availableSpins != storedSpins)
            {
                PlayerPrefs.SetInt(FreeSpinCountKey, availableSpins);
            }

            var now = DateTime.Now;

            if (maxSpins == 0)
            {
                if (!PlayerPrefs.HasKey(LastFreeSpinTimeKey))
                {
                    PlayerPrefs.SetString(LastFreeSpinTimeKey, now.ToString("o"));
                }

                return NotifyAndReturn(0);
            }

            var rechargeHours = Mathf.Max(0f, settings.freeLuckySpinRechargeHours);
            if (rechargeHours <= 0f)
            {
                if (availableSpins != maxSpins)
                {
                    availableSpins = maxSpins;
                    PlayerPrefs.SetInt(FreeSpinCountKey, availableSpins);
                }

                if (!PlayerPrefs.HasKey(LastFreeSpinTimeKey))
                {
                    PlayerPrefs.SetString(LastFreeSpinTimeKey, now.ToString("o"));
                }

                return NotifyAndReturn(availableSpins);
            }

            DateTime lastAccrualTime;
            if (!PlayerPrefs.HasKey(LastFreeSpinTimeKey) ||
                !DateTime.TryParse(PlayerPrefs.GetString(LastFreeSpinTimeKey), out lastAccrualTime))
            {
                lastAccrualTime = now;
                PlayerPrefs.SetString(LastFreeSpinTimeKey, lastAccrualTime.ToString("o"));
            }

            if (availableSpins >= maxSpins)
            {
                return NotifyAndReturn(maxSpins);
            }

            var secondsPerSpin = rechargeHours * 3600f;
            var elapsedSeconds = (now - lastAccrualTime).TotalSeconds;

            if (elapsedSeconds < 0)
            {
                elapsedSeconds = 0;
                lastAccrualTime = now;
                PlayerPrefs.SetString(LastFreeSpinTimeKey, lastAccrualTime.ToString("o"));
            }

            if (elapsedSeconds < secondsPerSpin)
            {
                return NotifyAndReturn(availableSpins);
            }

            var spinsToAdd = Mathf.Min(maxSpins - availableSpins, Mathf.FloorToInt((float)(elapsedSeconds / secondsPerSpin)));
            if (spinsToAdd > 0)
            {
                availableSpins += spinsToAdd;
                var remainderSeconds = elapsedSeconds - spinsToAdd * secondsPerSpin;
                var newTimestamp = now.AddSeconds(-remainderSeconds);
                PlayerPrefs.SetInt(FreeSpinCountKey, availableSpins);
                PlayerPrefs.SetString(LastFreeSpinTimeKey, newTimestamp.ToString("o"));
            }

            return NotifyAndReturn(availableSpins);
        }

        private void FreeSpin()
        {
            if (RefreshAvailableFreeSpins() <= 0)
            {
                UpdateButtonVisibility();
                return;
            }

            availableFreeSpins = Mathf.Max(0, availableFreeSpins - 1);
            PlayerPrefs.SetInt(FreeSpinCountKey, availableFreeSpins);
            PlayerPrefs.SetString(LastFreeSpinTimeKey, DateTime.Now.ToString("o"));
            freeSpinUpdateTimer = 0f;
            NotifyFreeSpinCountChanged(availableFreeSpins);
            UpdateFreeSpinStatusLabels();
            Spin();
        }

        private IEnumerator SwitchLightsAlpha()
        {
            const float maxSpeed = 100;

            while (true)
            {
                var speedRatio = Mathf.Abs(rb.angularVelocity) / maxSpeed; // Ratio of the current speed to the maximum speed
                speedRatio = Mathf.Min(speedRatio, .9f);
                var delay = 1f - speedRatio; // Higher speed -> smaller delay
                yield return new WaitForSeconds(delay);

                foreach (var light in lights)
                {
                    light.color = new Color(light.color.r, light.color.g, light.color.b, light.color.a == 0 ? 1 : 0);
                }
            }
        }

        public void DefineRewards(RewardSettingSpin[] spinRewards)
        {
            this.spinRewards = spinRewards;
            foreach (var reward in spinRewards)
            {
                var obj = Instantiate(reward.rewardVisualPrefab, spin.transform);
                //rotate to 360/number of rewards
                obj.transform.localPosition += new Vector3(0, 20, 0);
                obj.transform.RotateAround(spin.transform.position, Vector3.forward, 360f / spinRewards.Length * obj.transform.GetSiblingIndex());
                obj.SetCount(reward.count);
                rewards.Add(obj);
            }
        }

        public void Spin()
        {
            StartCoroutine(StartSpin());
        }

        private IEnumerator StartSpin()
        {
            // buttons interaction
            closeButton.interactable = false;
            freeSpinButton.interactable = false;
            rewardedAdButton.interactable = false;
            //hide buttons
            SetButtonsVisibility(false);

            RestoreInitialAngularDamping();
            var randomVelocity = CalculateRandomVelocity();

            float timeElapsed = 0;
            isSpinning = true;
            previousRotationMarker = Mathf.FloorToInt(spin.transform.eulerAngles.z / 25);

            while (timeElapsed < stoptime)
            {
                var appliedTorque = Mathf.Lerp(0, randomVelocity, timeElapsed / stoptime);
                rb.AddTorque(appliedTorque);
                timeElapsed += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            var stopDamping = initialAngularDamping > 0f
                ? initialAngularDamping * stopDampingMultiplier
                : stopDampingMultiplier;
            rb.angularDamping = stopDamping;
            yield return new WaitWhile(() => Mathf.Abs(rb.angularVelocity) > stopAngularVelocityThreshold);
            rb.angularDamping = initialAngularDamping;
            isSpinning = false;
            CheckReward(GetWinReward());
        }

        private float CalculateRandomVelocity()
        {
            var baseMultiplier = Random.Range(minVelocityMultiplier, maxVelocityMultiplier);
            var additionalRandomness = Random.Range(-additionalRandomFactor, additionalRandomFactor);
            return velocity * (baseMultiplier + additionalRandomness);
        }

        private void Update()
        {
            if (isSpinning)
            {
                CheckPlaySound();
            }
            else
            {
                freeSpinUpdateTimer += Time.deltaTime;
                if (freeSpinUpdateTimer >= 1f)
                {
                    freeSpinUpdateTimer = 0f;
                    UpdateButtonVisibility();
                }
            }
        }

        private void CacheInitialAngularDamping()
        {
            if (rb == null)
            {
                return;
            }

            if (!angularDampingInitialized)
            {
                initialAngularDamping = rb.angularDamping;
                angularDampingInitialized = true;
            }
            else
            {
                RestoreInitialAngularDamping();
            }
        }

        private void RestoreInitialAngularDamping()
        {
            if (!angularDampingInitialized || rb == null)
            {
                return;
            }

            rb.angularDamping = initialAngularDamping;
        }

        private void UpdateFreeSpinStatusLabels()
        {
            if (freeSpinCountLabel != null)
            {
                var freeSpinText = availableFreeSpins.ToString();
                freeSpinCountLabel.text = CombineBaseTextWithValue(freeSpinCountLabelBaseText, freeSpinText);
            }

            if (nextFreeSpinTimerLabel == null)
            {
                return;
            }

            var remaining = GetTimeUntilNextFreeSpin();
            if (!remaining.HasValue)
            {
                nextFreeSpinTimerLabel.text = CombineBaseTextWithValue(nextFreeSpinTimerLabelBaseText, "-");
            }
            else
            {
                var formattedTime = FormatTimeSpan(remaining.Value);
                nextFreeSpinTimerLabel.text = CombineBaseTextWithValue(nextFreeSpinTimerLabelBaseText, formattedTime);
            }
        }

        private TimeSpan? GetTimeUntilNextFreeSpin()
        {
            if (gameSettings == null)
            {
                return null;
            }

            var maxSpins = Mathf.Max(0, gameSettings.maxFreeLuckySpinCount);
            if (maxSpins == 0)
            {
                return null;
            }

            var rechargeHours = Mathf.Max(0f, gameSettings.freeLuckySpinRechargeHours);
            if (rechargeHours <= 0f)
            {
                return TimeSpan.Zero;
            }

            if (availableFreeSpins >= maxSpins)
            {
                return TimeSpan.Zero;
            }

            if (!PlayerPrefs.HasKey(LastFreeSpinTimeKey))
            {
                return TimeSpan.FromHours(rechargeHours);
            }

            if (!DateTime.TryParse(PlayerPrefs.GetString(LastFreeSpinTimeKey), out var lastAccrualTime))
            {
                return TimeSpan.FromHours(rechargeHours);
            }

            var secondsPerSpin = rechargeHours * 3600f;
            var elapsedSeconds = (DateTime.Now - lastAccrualTime).TotalSeconds;

            if (elapsedSeconds < 0)
            {
                elapsedSeconds = 0;
            }

            var remainingSeconds = Mathf.Max(0f, secondsPerSpin - (float)elapsedSeconds);
            return TimeSpan.FromSeconds(remainingSeconds);
        }

        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            var totalSeconds = (int)Mathf.Ceil((float)timeSpan.TotalSeconds);
            if (totalSeconds <= 0)
            {
                return "00:00:00";
            }

            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            var seconds = totalSeconds % 60;
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
        }

        private void CheckPlaySound()
        {
            var currentZRotation = spin.transform.eulerAngles.z;
            var currentTenDegreeMarker = Mathf.FloorToInt(currentZRotation / 25);

            if (currentTenDegreeMarker != previousRotationMarker)
            {
                audioService.PlaySound(luckySpin);
                previousRotationMarker = currentTenDegreeMarker;
            }
        }

        private int GetWinReward()
        {
            audioService.PlaySound(applause);
            var highestYIndex = 0; // Start with first item's index
            var highestY = rewards[0].transform.position.y; // and its 'y' position

            for (var i = 1; i < rewards.Count; i++)
            {
                // If current item's 'y' position is higher
                if (rewards[i].transform.position.y > highestY)
                {
                    highestY = rewards[i].transform.position.y;
                    highestYIndex = i;
                }
            }

            return highestYIndex;
        }

        private void CheckReward(int rewardIndex)
        {
            var rewardSettingSpin = spinRewards[rewardIndex];
            var rewardVisual = rewards[rewardIndex];
            var _resource = rewardSettingSpin.resource;
            var iconPos = rewardVisual.transform;
            var _count = rewardSettingSpin.count;
            
            // Analytics: Coins Earned (if it's a coin resource)
            var levelManager = FindObjectOfType<WordsToolkit.Scripts.Gameplay.Managers.LevelManager>();
            if (levelManager != null && _resource.name.ToLower().Contains("coin"))
            {
                Analytics.CoinsEarned(levelManager.currentLevel, _count);
            }
            
            _resource.AddAnimated(_count, iconPos.position, animationSourceObject: null, callback: () =>
            {
                //Close();
                closeButton.interactable = true;
                freeSpinButton.interactable = true;
                rewardedAdButton.interactable = true;
                
            });
        }
    }
}