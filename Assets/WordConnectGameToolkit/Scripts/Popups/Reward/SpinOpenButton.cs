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
using VContainer;
using TMPro;
using WordsToolkit.Scripts.Popups;
using WordsToolkit.Scripts.GUI;
using WordsToolkit.Scripts.GUI.Buttons;
using WordsToolkit.Scripts.Settings;

namespace WordsToolkit.Scripts.Popups.Reward
{
    public class SpinOpenButton : MonoBehaviour
    {
        [Inject]
        private MenuManager menuManager;

        [SerializeField]
        private CustomButton spinButton;

        [SerializeField]
        private TMP_Text freeSpinLabel;

        [Inject]
        private GameSettings gameSettings;

        private int displayedFreeSpinCount = int.MinValue;

        private void Awake()
        {
            EnsureDependencies();
        }

        private void OnEnable()
        {
            EnsureDependencies();

            if (spinButton != null)
            {
                spinButton.onClick.AddListener(ShowLuckySpin);
            }

            LuckySpin.FreeSpinCountChanged += OnFreeSpinCountChanged;

            UpdateFreeSpinCount();
        }

        private void OnDisable()
        {
            if (spinButton != null)
            {
                spinButton.onClick.RemoveListener(ShowLuckySpin);
            }

            LuckySpin.FreeSpinCountChanged -= OnFreeSpinCountChanged;
        }

        private void EnsureDependencies()
        {
            if (menuManager == null)
            {
#if UNITY_2023_1_OR_NEWER
                menuManager = FindAnyObjectByType<MenuManager>();
#else
                menuManager = FindObjectOfType<MenuManager>();
#endif
            }

            if (gameSettings == null)
            {
                gameSettings = Resources.Load<GameSettings>("Settings/GameSettings");
            }
        }

        private void CheckFree()
        {
            UpdateFreeSpinCount();
        }

        private void UpdateFreeSpinCount()
        {
            if (gameSettings == null)
            {
                Debug.LogWarning("SpinOpenButton: GameSettings dependency is missing, cannot update free spin label.");
                return;
            }

            var freeSpins = LuckySpin.GetAvailableFreeSpins(gameSettings);
            ApplyFreeSpinCount(freeSpins);
        }

        private void OnFreeSpinCountChanged(int freeSpinCount)
        {
            ApplyFreeSpinCount(freeSpinCount);
        }

        private void ApplyFreeSpinCount(int freeSpinCount)
        {
            if (freeSpinLabel == null)
            {
                return;
            }

            if (displayedFreeSpinCount == freeSpinCount)
            {
                return;
            }

            displayedFreeSpinCount = freeSpinCount;
            freeSpinLabel.text = freeSpinCount.ToString();
        }

        public void ShowLuckySpin()
        {
            if (menuManager == null)
            {
                Debug.LogError("SpinOpenButton: MenuManager dependency is missing, cannot open LuckySpin popup.");
                return;
            }

            menuManager.ShowPopup<LuckySpin>(null, x => CheckFree());
        }


    }
}
