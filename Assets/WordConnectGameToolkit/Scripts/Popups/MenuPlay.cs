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
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using WordsToolkit.Scripts.Levels;
using WordsToolkit.Scripts.System;
using VContainer;
using WordsToolkit.Scripts.Gameplay.Managers;
using WordsToolkit.Scripts.GUI;
using WordsToolkit.Scripts.GUI.Labels;
using WordsToolkit.Scripts.Enums;
using WordsToolkit.Scripts.Data;
using WordsToolkit.Scripts.Services;
using GamePush;
using WordsToolkit.Scripts.Infrastructure.DI;


namespace WordsToolkit.Scripts.Popups
{
    public class MenuPlay : Popup
    {
        public Image[] backgroundImages;
        public Slider scrollBar;
        public TextMeshProUGUI counter;
        public Button play;

        private LevelGroup currentGroup;
        private LevelGroup nextGroup;
        private Level currentLevel;

        [Inject]
        private SceneLoader sceneLoader;
        [Inject]
        private BackgroundChanger backgroundChanger;
        [Inject]
        private IBackgroundLoaderService backgroundLoaderService;
        [SerializeField]
        private GameObject hardLabel;
        private bool hasClaimedRewards = false;
        [SerializeField]
        [Tooltip("Reference to the Gems resource")]
        public ResourceObject gemsResource;
        [SerializeField]
        private Transform startAnimationTransform;

        private void OnEnable()
        {
            stateManager.HideMain();

            play.onClick.AddListener(Play);

            // Get the current level from GameDataManager
            currentLevel = GameDataManager.GetLevel();
            // If current level is null, try to find previous level
            if (currentLevel == null)
            {
                TryLoadPreviousLevel();
            }

            currentGroup = currentLevel?.GetGroup();
            nextGroup = currentLevel?.GetGroup().GetNextGroup();
            // Try to find a background sprite following the hierarchy
            LoadBackgroundsAsync();

            // Update progress UI elements
            UpdateProgressUI();
        }

        private void Start()
        {
            
        }

        public override void AfterShowAnimation()
        {
            base.AfterShowAnimation();
            if (currentLevel.isHardLevel)
            {
                hardLabel.SetActive(true); hardLabel.GetComponent<Animator>().Play("HardLabel");
            }
            ClaimLevelRewards();
        }

        private void TryLoadPreviousLevel()
        {
            Debug.LogWarning("Current level is null, trying to load previous level");

            // Get current level number
            int currentLevelNum = GameDataManager.GetLevelNum();

            // Try to find a valid previous level
            int previousLevel = currentLevelNum - 1;
            while (previousLevel > 0)
            {
                GameDataManager.SetLevelNum(previousLevel);
                Level levelData = GameDataManager.GetLevel();
                if (levelData != null)
                {
                    currentLevel = levelData;
                    Debug.Log($"Loaded previous level: {previousLevel}");
                    break;
                }
                previousLevel--;
            }

            // If we still couldn't find a valid level, log an error
            if (currentLevel == null)
            {
                Debug.LogError("Could not find any valid level to load");
            }
        }

        private void Play()
        {
            // Make sure we have a valid level to play
            if (currentLevel == null)
            {
                Debug.LogWarning("Attempting to play with null level, trying to load previous level");
                TryLoadPreviousLevel();

                // If still null, we can't play
                if (currentLevel == null)
                {
                    Debug.LogError("No valid level to play");
                    return;
                }
            }

            if (AdsSettings.InterPlacement == "preLevel" && levelManager.currentLevel >= AdsSettings.InterMinLevel)
            {
                Ads.ShowInter(() =>
                {
                    Analytics.AdImpression("interstitial", "preLevel");
                }, b =>
                {
                
                });
            }
            
            GameDataManager.SetLevel(currentLevel);
            sceneLoader.StartGameScene();
            Close();
        }

        private void UpdateProgressUI()
        {
            if (counter != null && currentLevel != null)
            {
                // Check if we have a valid group
                if (currentGroup == null || currentGroup.levels == null)
                {
                    counter.text = "0/0";
                    scrollBar.minValue = 0;
                    scrollBar.maxValue = 1;
                    scrollBar.value = 0;
                    return;
                }

                // Get the total number of levels in the group
                int totalLevels = currentGroup.levels.Count;

                // Find the index of current level in the group
                int currentIndex = currentGroup.levels.IndexOf(currentLevel);

                // If level is not found in the group, set index to 0
                if (currentIndex < 0)
                {
                    currentIndex = 0;
                }

                // Update counter and scrollbar
                counter.text = $"{currentIndex}/{totalLevels}";
                scrollBar.minValue = 0;
                scrollBar.maxValue = totalLevels;
                scrollBar.value = currentIndex+((float)scrollBar.maxValue/15);
            }
        }

        private async void LoadBackgroundsAsync()
        {
            try
            {
                Debug.Log("[MenuPlay] LoadBackgroundsAsync started");
                
                // Load current group background
                Debug.Log($"[MenuPlay] Loading background for current group: {currentGroup?.groupName}");
                Sprite backgroundToUse = await GetBackgroundFromHierarchyAsync(currentGroup);
                Debug.Log($"[MenuPlay] Background loaded: {backgroundToUse != null}");
                
                // Check if object was destroyed during async operation
                if (this == null || destroyCancellationToken.IsCancellationRequested)
                {
                    Debug.Log("[MenuPlay] Object was destroyed during async operation - skipping background setup");
                    return;
                }
                
                if (backgroundToUse != null && backgroundChanger != null)
                {
                    Debug.Log("[MenuPlay] Setting background via backgroundChanger");
                    backgroundChanger.SetBackground(backgroundToUse);
                    Debug.Log("[MenuPlay] Background set successfully");
                }

                // Load next group background for preview
                if (backgroundImages != null && backgroundImages.Length > 0)
                {
                    Debug.Log($"[MenuPlay] Loading background for next group: {nextGroup?.groupName}");
                    Sprite nextGroupBackground = await GetBackgroundFromHierarchyAsync(nextGroup);
                    Debug.Log($"[MenuPlay] Next group background loaded: {nextGroupBackground != null}");
                    
                    // Check again if object was destroyed
                    if (this == null || destroyCancellationToken.IsCancellationRequested)
                    {
                        Debug.Log("[MenuPlay] Object was destroyed during async operation (next group) - skipping");
                        return;
                    }
                    
                    if (nextGroupBackground != null)
                    {
                        Debug.Log($"[MenuPlay] Setting {backgroundImages.Length} background images");
                        for (int i = 0; i < backgroundImages.Length; i++)
                        {
                            if (backgroundImages[i] != null)
                            {
                                backgroundImages[i].sprite = nextGroupBackground;
                            }
                        }
                        Debug.Log("[MenuPlay] All background images set successfully");
                    }
                }
                
                Debug.Log("[MenuPlay] LoadBackgroundsAsync completed");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[MenuPlay] LoadBackgroundsAsync cancelled - object was destroyed");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MenuPlay] Exception in LoadBackgroundsAsync: {e.Message}");
                Debug.LogError($"[MenuPlay] Stack trace: {e.StackTrace}");
            }
        }

        private async UniTask<Sprite> GetBackgroundFromHierarchyAsync(LevelGroup currentGroup)
        {
            if (currentGroup == null) return null;

            // Start from the top-most parent
            LevelGroup current = currentGroup;
            while (current.parentGroup != null)
            {
                var backgroundRef = current.parentGroup.GetBackgroundReference();
                if (backgroundRef != null)
                {
                    return await backgroundLoaderService.LoadBackgroundAsync(backgroundRef);
                }
                current = current.parentGroup;
            }

            // Then check current group
            var currentGroupBackgroundRef = currentGroup.GetBackgroundReference();
            if (currentGroupBackgroundRef != null)
            {
                return await backgroundLoaderService.LoadBackgroundAsync(currentGroupBackgroundRef);
            }

            // Finally check the level itself
            if (currentLevel != null)
            {
                var levelBackgroundRef = currentLevel.GetBackgroundReference();
                if (levelBackgroundRef != null)
                {
                    return await backgroundLoaderService.LoadBackgroundAsync(levelBackgroundRef);
                }
                
                Debug.LogError("Background null from hierarchu async");
            }

            return null;
        }
        


        private void ClaimLevelRewards()
        {
            if (hasClaimedRewards)
                return;

            Level currentLevel = GameDataManager.GetLevel();
            LevelGroup currentGroup = currentLevel?.GetGroup();
            int zeroBasedIndex = currentGroup.levels.IndexOf(currentLevel);

            if (zeroBasedIndex == currentGroup.levels.Count / 2)
            {
                // Mark as claimed
                hasClaimedRewards = true;
                GP_PlayerWrapper.Set("ExtraWordsCollected", 0); // Reset count after claiming
                EventManager.GetEvent(EGameEvent.ExtraWordClaimed).Invoke();
                // Use manually assigned reference or fall back to ResourceManager if not assigned
                var gems = gemsResource != null ? gemsResource : resourceManager.GetResource("Gems");

                // Add reward with animation
                if (gems != null)
                {
                    const int rewardAmount = 50;

                    // Add immediately so reward isn't lost if animation is interrupted
                    gems.Add(rewardAmount);

                    if (startAnimationTransform != null)
                    {
                        ResourceAnimationController.AnimateForResource(
                            gems,
                            null,
                            startAnimationTransform.position,
                            "+" + rewardAmount,
                            gems.sound,
                            null);
                    }
                }
            }




        }

        
    }
}