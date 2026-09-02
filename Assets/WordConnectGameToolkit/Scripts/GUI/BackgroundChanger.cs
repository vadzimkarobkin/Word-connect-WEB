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
using UnityEngine.UI;
using VContainer;
using Cysharp.Threading.Tasks;
using GamePush;
using WordsToolkit.Scripts.Gameplay.Managers;
using WordsToolkit.Scripts.Infrastructure.Service;
using WordsToolkit.Scripts.Levels;
using WordsToolkit.Scripts.Popups;
using WordsToolkit.Scripts.System;
using WordsToolkit.Scripts.Services;

namespace WordsToolkit.Scripts.GUI
{
    public class BackgroundChanger : MonoBehaviour
    {
        [SerializeField]
        private Image image;

        private ILevelLoaderService levelLoaderService;
        private StateManager stateManager;
        private IBackgroundLoaderService backgroundLoaderService;
        [SerializeField]
        private Sprite mainBackground;

        [Inject]
        public void Construct(ILevelLoaderService levelLoaderService, StateManager stateManager, IBackgroundLoaderService backgroundLoaderService)
        {
            this.levelLoaderService = levelLoaderService;
            this.stateManager = stateManager;
            this.backgroundLoaderService = backgroundLoaderService;
            
            if (this.levelLoaderService != null)
            {
                this.levelLoaderService.OnLevelLoaded += OnLevelLoaded;
            }
            else
            {
                Debug.LogError("[BackgroundChanger] LevelLoaderService is null!");
            }
            
            SceneLoader.OnGameStart += SetBack;
        }

        private void OnEnable()
        {
            stateManager.OnStateChanged.AddListener(OnStateChanged);
        }

        private void OnStateChanged(EScreenStates arg0)
        {
            if (arg0 == EScreenStates.Game)
            {
                SetBack();
            }else if (arg0 == EScreenStates.MainMenu)
            {
                image.sprite = mainBackground;
            }
        }

        private async void SetBack()
        {
            try
            {
                Debug.Log("[BackgroundChanger] SetBack called");
                var level = GameDataManager.GetLevel();
                Debug.Log($"[BackgroundChanger] Level: {level?.number}");
                
                if (level != null)
                {
                    Debug.Log($"[BackgroundChanger] Loading background for level: {level.number}");
                    var background = await backgroundLoaderService.LoadBackgroundAsync(level);
                    Debug.Log($"[BackgroundChanger] Background loaded: {background != null}");
                    
                    // Check if this object was destroyed during async operation
                    if (this == null || image == null)
                    {
                        Debug.LogError("[BackgroundChanger] Object or image was destroyed during async operation");
                        return;
                    }
                    
                    if (background != null)
                    {
                        Debug.Log($"[BackgroundChanger] About to set image.sprite");
                        Debug.Log($"[BackgroundChanger] image component state: {image != null}, gameObject active: {image?.gameObject?.activeInHierarchy}");
                        image.sprite = background;
                        Debug.Log($"[BackgroundChanger] image.sprite set successfully");
                    }
                    else
                    {
                        Debug.LogWarning($"[BackgroundChanger] Background is null for level: {level.number}");
                    }
                }
                else
                {
                    Debug.LogWarning("[BackgroundChanger] Level is null");
                }
            }
            catch (SystemException e)
            {
                Debug.LogError($"[BackgroundChanger] Exception in SetBack: {e.Message}");
                Debug.LogError($"[BackgroundChanger] Stack trace: {e.StackTrace}");
            }
        }

        private void OnDestroy()
        {
            if (levelLoaderService != null)
            {
                levelLoaderService.OnLevelLoaded -= OnLevelLoaded;
                SceneLoader.OnGameStart -= SetBack;
                stateManager.OnStateChanged.RemoveListener(OnStateChanged);
            }
        }

        private async void OnLevelLoaded(Level level)
        {
            try
            {
                Debug.Log($"[BackgroundChanger] OnLevelLoaded called with level: {level?.number}");
                var background = await backgroundLoaderService.LoadBackgroundAsync(level);
                Debug.Log($"[BackgroundChanger] OnLevelLoaded - Background loaded: {background != null}");
                
                // Check if this object was destroyed during async operation
                if (this == null || image == null)
                {
                    Debug.LogError("[BackgroundChanger] OnLevelLoaded - Object or image was destroyed during async operation");
                    return;
                }
                
                if (background != null)
                {
                    Debug.Log($"[BackgroundChanger] OnLevelLoaded - About to set image.sprite");
                    Debug.Log($"[BackgroundChanger] OnLevelLoaded - image component state: {image != null}, gameObject active: {image?.gameObject?.activeInHierarchy}");
                    image.sprite = background;
                    Debug.Log($"[BackgroundChanger] OnLevelLoaded - image.sprite set successfully");
                }
                else
                {
                    Debug.LogWarning($"[BackgroundChanger] OnLevelLoaded - Background is null for level: {level?.number}");
                }
            }
            catch (SystemException e)
            {
                Debug.LogError($"[BackgroundChanger] Exception in OnLevelLoaded: {e.Message}");
                Debug.LogError($"[BackgroundChanger] Stack trace: {e.StackTrace}");
            }
        }

        public void SetBackground(Sprite bg)
        {
            Debug.Log($"[BackgroundChanger] SetBackground called with sprite: {bg != null}");
            Debug.Log($"[BackgroundChanger] Image component: {image != null}");
            Debug.Log($"[BackgroundChanger] Image gameObject: {image?.gameObject != null}");
            Debug.Log($"[BackgroundChanger] Image gameObject active: {image?.gameObject.activeInHierarchy}");
            
            image.sprite = bg;
            Debug.Log($"[BackgroundChanger] SetBackground - image.sprite set successfully");
        }
    }
}