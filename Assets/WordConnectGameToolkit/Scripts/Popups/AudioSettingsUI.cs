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
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;
using WordsToolkit.Scripts.Audio;
using GamePush;
using UnityEngine.EventSystems;
using WordsToolkit.Scripts.System;

namespace WordsToolkit.Scripts.Popups
{
    public class AudioSettingsUI : MonoBehaviour
    {
        [SerializeField]
        private Slider musicButton;

        [SerializeField]
        private Slider soundButton;

        [SerializeField]
        private AudioMixer mixer;

        [SerializeField]
        private string musicParameter = "musicVolume";

        [SerializeField]
        private string soundParameter = "soundVolume";

        private Button musicHandleButton;

        private Button soundHandleButton;

        private void Start()
        {
            musicButton.onValueChanged.AddListener(ToggleMusic);
            soundButton.onValueChanged.AddListener(ToggleSound);
            SetupSliderHandleToggle(musicButton, ref musicHandleButton, ToggleMusicHandle);
            SetupSliderHandleToggle(soundButton, ref soundHandleButton, ToggleSoundHandle);
            OnEnable();
        }

        private void OnEnable()
        {
            UpdateButtonState(musicButton, "Music", musicParameter);
            UpdateButtonState(soundButton, "Sound", soundParameter);
        }

        private void OnDestroy()
        {
            musicButton.onValueChanged.RemoveListener(ToggleMusic);
            soundButton.onValueChanged.RemoveListener(ToggleSound);

            if (musicHandleButton != null)
            {
                musicHandleButton.onClick.RemoveListener(ToggleMusicHandle);
            }

            if (soundHandleButton != null)
            {
                soundHandleButton.onClick.RemoveListener(ToggleSoundHandle);
            }
        }

        private void UpdateButtonState(Slider slider, string playerPrefKey, string volumeParameter)
        {
            EnsurePreferenceInitialized(playerPrefKey);

            var enabledState = GP_PlayerWrapper.GetInt(playerPrefKey, 1) != 0;
            float volumeValue = enabledState ? 0 : -80;

            mixer.SetFloat(volumeParameter, volumeValue);
            if (slider != null)
            {
                slider.value = enabledState ? 1 : 0;
            }
        }

        private void ToggleMusic(float arg0)
        {
            PersistAudioPreference("Music", arg0);
            OnEnable();
        }

        private void ToggleSound(float arg0)
        {
            PersistAudioPreference("Sound", arg0);
            OnEnable();
        }

        private void EnsurePreferenceInitialized(string key)
        {
            if (!GP_PlayerWrapper.Has(key))
            {
                GP_PlayerWrapper.Set(key, 1);
            }

            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
            }
        }

        private void PersistAudioPreference(string key, float sliderValue)
        {
            int normalizedValue = sliderValue >= 0.5f ? 1 : 0;

            GP_PlayerWrapper.Set(key, normalizedValue);
            PlayerPrefs.SetInt(key, normalizedValue);
            PlayerPrefs.Save();
        }

        private void SetupSliderHandleToggle(Slider slider, ref Button handleButton, UnityAction toggleAction)
        {
            if (slider == null || slider.handleRect == null)
            {
                handleButton = null;
                return;
            }

            handleButton = slider.handleRect.GetComponent<Button>();

            if (handleButton == null)
            {
                handleButton = slider.handleRect.gameObject.AddComponent<Button>();
                handleButton.transition = Selectable.Transition.None;
            }

            handleButton.onClick.RemoveListener(toggleAction);
            handleButton.onClick.AddListener(toggleAction);
        }

        private void ToggleMusicHandle()
        {
            ToggleSliderValue(musicButton);
        }

        private void ToggleSoundHandle()
        {
            ToggleSliderValue(soundButton);
        }

        private void ToggleSliderValue(Slider slider)
        {
            if (slider == null)
                return;

            var newValue = slider.value < 0.5f ? 1.0f : 0.0f;
            slider.value = newValue;
        }
    }
}
