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

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using VContainer;
using WordsToolkit.Scripts.Levels;
using WordsToolkit.Scripts.System;
using WordsToolkit.Scripts.Services;
using System;

namespace WordsToolkit.Scripts.Popups
{
    public class Finish : Popup
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        private Level currentLevel;
        [SerializeField]
        private AudioClip swish;

        [SerializeField]
        private AudioClip cheers;

        [SerializeField]
        private Image background;
        
        [Inject]
        private IBackgroundLoaderService backgroundLoaderService;

        private async void OnEnable()
        {
            currentLevel = levelManager.GetCurrentLevel();
            //LevelGroup nextGroup = levelManager.GetCurrentLevel().GetGroup.nextGroup();
            LevelGroup nextGroup = currentLevel?.GetGroup().GetNextGroup();

            SetFinishText(nextGroup.GetTitle(gameManager.language),
                nextGroup.GetText(gameManager.language));

            // Load background asynchronously
            try
            {
                if (nextGroup != null)
                {
                    Debug.Log($"[Finish] Loading background for next group: {nextGroup.groupName}");
                    var backgroundSprite = await backgroundLoaderService.LoadBackgroundAsync(nextGroup);
                    Debug.Log($"[Finish] Background loaded: {backgroundSprite != null}");
                    
                    if (this == null)
                    {
                        Debug.LogError("[Finish] Object was destroyed during async operation");
                        return;
                    }
                    
                    if (backgroundSprite != null)
                    {
                        Debug.Log($"[Finish] Setting background.sprite, background component: {background != null}");
                        if (background != null)
                        {
                            background.sprite = backgroundSprite;
                            Debug.Log("[Finish] Background sprite set successfully");
                        }
                        else
                        {
                            Debug.LogError("[Finish] background Image component is null!");
                        }
                    }
                }
            }
            catch (SystemException e)
            {
                Debug.LogError($"[Finish] Exception loading background: {e.Message}");
                Debug.LogError($"[Finish] Stack trace: {e.StackTrace}");
            }
        }

        private void SetFinishText(string titleText, string descriptionText)
        {
            if (titleText != "")
            {
                title.text = titleText;
            }

            if (descriptionText != "")
            {
                description.text = descriptionText;
            }
        }

        public override void ShowAnimationSound()
        {
            base.ShowAnimationSound();
            audioService.PlaySound(cheers);
        }
    }
}