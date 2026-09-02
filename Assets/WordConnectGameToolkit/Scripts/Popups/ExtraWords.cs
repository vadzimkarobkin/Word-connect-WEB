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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using WordsToolkit.Scripts.Data;
using WordsToolkit.Scripts.Enums;
using WordsToolkit.Scripts.GUI.Buttons;
using WordsToolkit.Scripts.System;

namespace WordsToolkit.Scripts.Popups
{
    public class ExtraWords : Popup
    {
        [Header("Word Display")]
        // Format for displaying each word in the list
        [Tooltip("Format for each word in the list. Use {0} for the word.")]
        public string wordFormat = "• {0}";
        
        // Separator between words
        [Tooltip("String to separate words in the list")]
        public string separator = "\n";
        
        [Header("Font Size")]
        [Tooltip("Fixed font size for all text")]
        [Range(8, 72)]
        public float fixedFontSize = 60f;
        
        [Tooltip("Enable font size adjustment to fit container")]
        public bool autoFitContainer = true;
        
        [Header("Columns")]
        [Tooltip("List of pre-existing TextMeshPro components to use as columns")]
        public TextMeshProUGUI[] columnTextObjects;
        
        [Tooltip("Maximum lines per column (0 = unlimited)")]
        [Range(0, 30)]
        public int maxLinesPerColumn = 6;

        [Tooltip("Line spacing multiplier")]
        [Range(0.5f, 2f)]
        public float lineSpacingMultiplier = 1f;

        [Header("Rewards")]
        [Tooltip("Button to claim gems for found extra words")]
        public CustomButton claimButton;
        public CustomButton claimButtonX2;
        public CustomButton closeButton;
        public CustomButton closeButtonCross;
        
        [Tooltip("Text to show total gems to be claimed")]
        public TextMeshProUGUI rewardText;
        
        [Tooltip("Reference to the Gems resource")]
        public ResourceObject gemsResource;

        private bool hasClaimedRewards = false;
        [SerializeField]
        private Transform startAnimationTransform;
        private string plus = "+";


        protected void OnEnable()
        {
            // Configure text components on enable
            SetupTextComponents();
            UpdateExtraWordsDisplay();
            SetupClaimButton();
        }

        protected override void Awake()
        {
            base.Awake();

            if (closeButtonCross != null)
            {
                closeButtonCross.onClick.RemoveListener(Close);
                closeButtonCross.onClick.AddListener(Close);
            }
        }
        
        private void SetupTextComponents()
        {
            // Apply settings to existing text components
            if (columnTextObjects == null || columnTextObjects.Length == 0)
                return;
            
            foreach (var text in columnTextObjects)
            {
                if (text == null)
                    continue;
                
                // Configure text formatting
                text.enableWordWrapping = true;
                text.overflowMode = TextOverflowModes.Ellipsis;
                
                // Set font size with auto-fitting options
                if (autoFitContainer)
                {
                    text.enableAutoSizing = true;
                    text.fontSizeMax = fixedFontSize;
                    text.fontSizeMin = fixedFontSize * 0.5f;
                }
                else
                {
                    text.fontSize = fixedFontSize;
                    text.enableAutoSizing = false;
                }
                
                // Clear any existing content
                text.text = string.Empty;
                
                // Hide all initially
                text.gameObject.SetActive(false);
            }
        }

        private void SetupClaimButton()
        {
            // Reset claim state so the player can claim rewards again when the popup opens
            hasClaimedRewards = false;

            // Pending coins are equal to the amount of extra words found since the last claim
            int pendingCoins = Mathf.Max(0, GP_PlayerWrapper.GetInt("ExtraWordsCollected"));

            // Show total reward or hide buttons if no rewards are available
            bool hasRewardsToClaim = pendingCoins > 0;

            if (claimButton != null)
            {
                // Setup button click handler
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(() => ClaimExtraWordRewards());
                claimButton.gameObject.SetActive(hasRewardsToClaim);
            }

            if (claimButtonX2 != null)
            {
                claimButtonX2.gameObject.SetActive(hasRewardsToClaim);
            }

            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(!hasRewardsToClaim);
            }

            // Update reward text if available
            if (rewardText != null)
            {
                rewardText.text = plus + pendingCoins.ToString();
            }
        }
        
        public void ClaimX2()
        {
            ClaimExtraWordRewards(2);
        }

        private void ClaimExtraWordRewards(int mod = 1)
        {
            if (hasClaimedRewards)
                return;

            int pendingCoins = Mathf.Max(0, GP_PlayerWrapper.GetInt("ExtraWordsCollected"));

            if (pendingCoins <= 0)
                return;

            // Mark as claimed
            hasClaimedRewards = true;
            GP_PlayerWrapper.Set("ExtraWordsCollected", 0); // Reset count after claiming
            EventManager.GetEvent(EGameEvent.ExtraWordClaimed).Invoke();
            // Use manually assigned reference or fall back to ResourceManager if not assigned
            var gems = gemsResource != null ? gemsResource : resourceManager.GetResource("Gems");

            // Add reward with animation
            var animationTransform = startAnimationTransform != null
                ? startAnimationTransform
                : claimButton != null
                    ? claimButton.transform
                    : transform;

            if (gems != null && animationTransform != null)
            {
                gems.AddAnimated(pendingCoins * mod, animationTransform.position, animationSourceObject: null, callback: () =>
                {
                    if (claimButton != null)
                    {
                        claimButton.gameObject.SetActive(false);
                    }

                    if (claimButtonX2 != null)
                    {
                        claimButtonX2.gameObject.SetActive(false);
                    }

                    Close();
                });
            }
            else if (gems != null)
            {
                var coinsEarned = pendingCoins * mod;
                gems.Add(coinsEarned);
                
                // Analytics: Coins Earned
                var levelManager = FindObjectOfType<WordsToolkit.Scripts.Gameplay.Managers.LevelManager>();
                if (levelManager != null)
                {
                    Analytics.CoinsEarned(levelManager.currentLevel, coinsEarned);
                }
                
                Close();
            }

            if (rewardText != null)
            {
                rewardText.text = "+0";
            }

            if (closeButton != null)
            {
                //closeButton.gameObject.SetActive(true);
            }

        }

#if UNITY_EDITOR
        private void Update()
        {
            // F5 to add a test word
            if (Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame)
            {
                AddTestWord();
            }
        }
#endif

        // Context menu item for testing reward claim in the Unity editor
        [ContextMenu("Test Reward Claim")]
        private void TestRewardClaim()
        {
            // Reset claim state to ensure we can claim again
            hasClaimedRewards = false;

            // Make sure the claim button is visible
            if (claimButton != null)
                claimButton.gameObject.SetActive(true);

            if (claimButtonX2 != null)
                claimButtonX2.gameObject.SetActive(true);

            int pendingCoins = Mathf.Max(1, GP_PlayerWrapper.GetInt("ExtraWordsCollected"));
            GP_PlayerWrapper.Set("ExtraWordsCollected", pendingCoins);

            // Set up reward text if available
            if (rewardText != null)
            {
                rewardText.text = plus+pendingCoins.ToString();
            }

            // Call the claim method to test the full reward process
            ClaimExtraWordRewards();
        }

        // Context menu item for adding a test word
        [ContextMenu("Add Test Word")]
        private void AddTestWord()
        {
            if (customWordRepository != null)
            {
                customWordRepository.AddExtraWord("blablabla " + Random.Range(1, 1000));
                UpdateExtraWordsDisplay();
                SetupClaimButton();
            }
        }

        // Updates the text components with current extra words
        public void UpdateExtraWordsDisplay()
        {
            // Get word list from game
            List<string> words = GetWordsList();
            
            // Early exit if no columns are assigned
            if (columnTextObjects == null || columnTextObjects.Length == 0)
                return;
                
            // If no words found, display message in first column
            if (words.Count == 0)
            {
                return;
            }
            
            // Otherwise, distribute words among columns
            DistributeWordsToColumns(words);
        }

        private void DistributeWordsToColumns(List<string> words)
        {
            // Count valid columns
            int validColumnCount = 0;
            foreach (var col in columnTextObjects)
            {
                if (col != null)
                    validColumnCount++;
            }
            
            if (validColumnCount == 0)
                return;
                
            int currentColumn = 0;
            int wordIndex = 0;
            
            // First, disable all columns
            foreach (var col in columnTextObjects)
            {
                if (col != null)
                    col.gameObject.SetActive(false);
            }
            
            while (wordIndex < words.Count && currentColumn < columnTextObjects.Length)
            {
                TextMeshProUGUI col = columnTextObjects[currentColumn];
                if (col == null)
                {
                    currentColumn++;
                    continue;
                }
                
                // Show this column
                col.gameObject.SetActive(true);
                
                // Build text for this column
                StringBuilder sb = new StringBuilder();
                int wordCount = 0;
                bool columnIsFull = false;
                
                // Fill this column until max lines or out of words
                while (!columnIsFull && wordIndex < words.Count)
                {
                    // Check if we've reached the max lines for this column
                    if (maxLinesPerColumn > 0 && wordCount >= maxLinesPerColumn)
                    {
                        columnIsFull = true;
                        break;
                    }
                    
                    if (wordCount > 0)
                        sb.Append(separator);
                    sb.Append(string.Format(wordFormat, words[wordIndex]));
                    wordIndex++;
                    wordCount++;
                }
                
                // Set text content
                col.text = sb.ToString();
                
                // Move to next column if we have more words or the current column is full
                if (wordIndex < words.Count || columnIsFull)
                {
                    currentColumn++;
                }
            }
        }
        
        // Gets the list of extra words from the level manager
        private List<string> GetWordsList()
        {
            if (levelManager != null && customWordRepository != null)
            {
                List<string> words = customWordRepository.GetExtraWords().Where(word => word != null).ToList();
                if (words.Count > 24)
                {
                    words = words.Skip(Mathf.Max(0, words.Count - 24)).ToList();
                }
                return words ?? new List<string>();
            }
            
            return new List<string>();
        }

        // Helper method to get target extra words from the current level's group or fallback to game settings
        private int GetTargetExtraWordsFromGroup()
        {
            var currentLevelGroup =  GameDataManager.GetLevel().GetGroup();
            return Mathf.Max(1, currentLevelGroup.targetExtraWords); // Ensure it's at least 1
        }
    }
}