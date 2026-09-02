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
using UnityEngine.UI;
using WordsToolkit.Scripts.Enums;
using WordsToolkit.Scripts.Gameplay.Managers;
using WordsToolkit.Scripts.GUI;
using WordsToolkit.Scripts.System;
using VContainer;
using WordsToolkit.Scripts.GUI.Buttons;
using WordsToolkit.Scripts.Services;
using GamePush;

namespace WordsToolkit.Scripts.Popups
{
    public class Settings : PopupWithCurrencyLabel
    {
        [SerializeField]
        private CustomButton privacypolicy;

        [SerializeField]
        private CustomButton googleUMPConsent;

        [SerializeField]
        private Button restorePurchase;

        [SerializeField]
        private Slider vibrationSlider;

        private Button vibrationHandleButton;

        private const string VibrationPrefKey = "VibrationLevel";


        protected virtual void OnEnable()
        {
            privacypolicy?.onClick.AddListener(PrivacyPolicy);
            googleUMPConsent?.onClick.AddListener(ReconsiderGoogleUMPConsent);
            LoadVibrationLevel();
            vibrationSlider.onValueChanged.AddListener(SaveVibrationLevel);
            SetupVibrationHandleToggle();
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(BackToGame);
            restorePurchase?.onClick.AddListener(RestorePurchase);
            restorePurchase?.gameObject.SetActive(gameSettings.enableInApps);
        }

        private void RestorePurchase()
        {
            gameManager.RestorePurchases(((b, list) =>
            {
                if (b)
                    Close();
            }));
        }

        private void BackToGame()
        {
            DisablePause();
            Close();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            vibrationSlider.onValueChanged.RemoveListener(SaveVibrationLevel);
            if (vibrationHandleButton != null)
            {
                vibrationHandleButton.onClick.RemoveListener(ToggleVibrationSlider);
            }
        }

        private void SaveVibrationLevel(float value)
        {
            GP_PlayerWrapper.Set(VibrationPrefKey, value);
        }

        private void SetupVibrationHandleToggle()
        {
            if (vibrationSlider == null || vibrationSlider.handleRect == null)
            {
                vibrationHandleButton = null;
                return;
            }

            vibrationHandleButton = vibrationSlider.handleRect.GetComponent<Button>();

            if (vibrationHandleButton == null)
            {
                vibrationHandleButton = vibrationSlider.handleRect.gameObject.AddComponent<Button>();
                vibrationHandleButton.transition = Selectable.Transition.None;
            }

            vibrationHandleButton.onClick.RemoveListener(ToggleVibrationSlider);
            vibrationHandleButton.onClick.AddListener(ToggleVibrationSlider);
        }

        private void ToggleVibrationSlider()
        {
            if (vibrationSlider == null)
                return;

            var newValue = vibrationSlider.value < 0.5f ? 1.0f : 0.0f;
            vibrationSlider.value = newValue;
        }

        private void LoadVibrationLevel()
        {
            if (GP_PlayerWrapper.Has(VibrationPrefKey))
            {
                vibrationSlider.value = GP_PlayerWrapper.GetFloat(VibrationPrefKey);
            }
            else
            {
                vibrationSlider.value = 1.0f;
                SaveVibrationLevel(1.0f);
            }
        }

        private void PrivacyPolicy()
        {
            StopInteration();
            DisablePause();
            menuManager.ShowPopup<GDPR>();
            Close();
        }

        private void ReconsiderGoogleUMPConsent()
        {
            StopInteration();
            DisablePause();
            adsManager.ReconsiderUMPConsent();
            Close();
        }

        private void DisablePause()
        {
            if (stateManager.CurrentState == EScreenStates.Game)
            {
                EventManager.GameStatus = EGameState.Playing;
            }
        }
    }
}