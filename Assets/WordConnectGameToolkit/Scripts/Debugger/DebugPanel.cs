using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using GamePush;
using VContainer;
using VContainer.Unity;
using WordsToolkit.Scripts.Data;
using WordsToolkit.Scripts.GUI.Buttons;
using WordsToolkit.Scripts.Infrastructure.DI;
using WordsToolkit.Scripts.Services;
using WordsToolkit.Scripts.Levels;
using WordsToolkit.Scripts.System;
using WordsToolkit.Scripts.Gameplay.Managers;

namespace WordsToolkit.Scripts.Popups
{
    public class DebugPanel : Popup
    {

        [SerializeField]
        private TMP_InputField levelInputField;

        [SerializeField]
        private TMP_Text pendingLevelLabel;

        [SerializeField]
        private CustomButton applyLevelButton;

        [SerializeField]
        private CustomButton nextLevelButton;

        [SerializeField]
        private CustomButton previousLevelButton;

        [SerializeField]
        private CustomButton instantWinButton;

        [SerializeField]
        private CustomButton setRussianLanguageButton;

        [SerializeField]
        private CustomButton grantCoinsButton;

        [SerializeField]
        private CustomButton spendAllCoinsButton;


        private static int? pendingLevelNumber;
        private static event Action PendingLevelChanged;

        private Coroutine closeFallbackRoutine;
        private bool closeCompleted;



        private void OnEnable()
        {
            closeCompleted = false;
            RegisterButtonCallbacks();
            RefreshInputField();
            RefreshPendingLabel();
            PendingLevelChanged += RefreshInputField;
            PendingLevelChanged += RefreshPendingLabel;
            if (levelInputField != null)
            {
                levelInputField.onSubmit.AddListener(HandleLevelInputSubmitted);
            }

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (closeFallbackRoutine != null)
            {
                StopCoroutine(closeFallbackRoutine);
                closeFallbackRoutine = null;
            }

            UnregisterButtonCallbacks();
            PendingLevelChanged -= RefreshInputField;
            PendingLevelChanged -= RefreshPendingLabel;
            if (levelInputField != null)
            {
                levelInputField.onSubmit.RemoveListener(HandleLevelInputSubmitted);
            }
        }

        /// <summary>
        /// Queues the provided level number to be loaded after the current level is completed.
        /// </summary>
        /// <param name="levelNumber">Number of the level to queue.</param>
        public void LoadLevelByNumber(int levelNumber)
        {
            QueueLevel(levelNumber);
        }

        /// <summary>
        /// Reads the desired level number from the linked input field and queues it.
        /// </summary>
        public void LoadLevelFromInput()
        {
            if (levelInputField == null)
            {
                Debug.LogWarning("[DebugPanel] Level input field is not assigned.");
                return;
            }

            if (!int.TryParse(levelInputField.text, out int levelNumber))
            {
                Debug.LogWarning($"[DebugPanel] Unable to parse level number from '{levelInputField.text}'.");
                return;
            }

            QueueLevel(levelNumber);
        }

        private void HandleLevelInputSubmitted(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return;
            }

            if (!int.TryParse(rawValue, out int levelNumber))
            {
                Debug.LogWarning($"[DebugPanel] Unable to parse level number from '{rawValue}'.");
                RefreshInputField();
                return;
            }

            QueueLevel(levelNumber);
        }

        /// <summary>
        /// Queues the next level relative to the currently selected one.
        /// </summary>
        public void LoadNextLevel()
        {
            int baseLevel = pendingLevelNumber ?? GameDataManager.GetLevelNum();
            QueueLevel(baseLevel + 1);
        }

        /// <summary>
        /// Queues the previous level relative to the currently selected one.
        /// </summary>
        public void LoadPreviousLevel()
        {
            int baseLevel = pendingLevelNumber ?? GameDataManager.GetLevelNum();
            QueueLevel(Mathf.Max(1, baseLevel - 1));
        }

        /// <summary>
        /// Applies the queued level number after the player wins the current level.
        /// </summary>
        /// <param name="levelNumber">The level number to queue.</param>
        private void QueueLevel(int levelNumber)
        {
            if (!ValidateLevel(levelNumber))
            {
                return;
            }

            pendingLevelNumber = levelNumber;
            Debug.Log($"[DebugPanel] Level {levelNumber} queued. It will be loaded after completing the current level.");

            PendingLevelChanged?.Invoke();
        }

        private void RegisterButtonCallbacks()
        {
            if (applyLevelButton != null)
            {
                applyLevelButton.onClick.AddListener(LoadLevelFromInput);
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.AddListener(LoadNextLevel);
            }

            if (previousLevelButton != null)
            {
                previousLevelButton.onClick.AddListener(LoadPreviousLevel);
            }

            if (instantWinButton != null)
            {
                instantWinButton.onClick.AddListener(HandleInstantWinRequested);
            }

            if (setRussianLanguageButton != null)
            {
                setRussianLanguageButton.onClick.AddListener(HandleSetRussianLanguageRequested);
            }

            if (grantCoinsButton != null)
            {
                grantCoinsButton.onClick.AddListener(HandleGrantCoinsRequested);
            }

            if (spendAllCoinsButton != null)
            {
                spendAllCoinsButton.onClick.AddListener(HandleSpendAllCoinsRequested);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HandleCloseRequested);
            }
        }

        private void UnregisterButtonCallbacks()
        {
            if (applyLevelButton != null)
            {
                applyLevelButton.onClick.RemoveListener(LoadLevelFromInput);
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.RemoveListener(LoadNextLevel);
            }

            if (previousLevelButton != null)
            {
                previousLevelButton.onClick.RemoveListener(LoadPreviousLevel);
            }

            if (instantWinButton != null)
            {
                instantWinButton.onClick.RemoveListener(HandleInstantWinRequested);
            }

            if (setRussianLanguageButton != null)
            {
                setRussianLanguageButton.onClick.RemoveListener(HandleSetRussianLanguageRequested);
            }

            if (grantCoinsButton != null)
            {
                grantCoinsButton.onClick.RemoveListener(HandleGrantCoinsRequested);
            }

            if (spendAllCoinsButton != null)
            {
                spendAllCoinsButton.onClick.RemoveListener(HandleSpendAllCoinsRequested);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HandleCloseRequested);
            }
        }

        private void HandleSetRussianLanguageRequested()
        {
            try
            {
                GP_Language.Change("ru");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DebugPanel] Failed to request GamePush language change: {ex.Message}");
            }

            var languageService = ResolveFromContainer<ILanguageService>();
            if (languageService != null)
            {
                try
                {
                    languageService.SetLanguage("ru");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[DebugPanel] Failed to apply Russian language via LanguageService: {ex.Message}");
                }
            }
        }

        private void HandleGrantCoinsRequested()
        {
            var resourceManager = ResolveResourceManager();
            var coins = GetCoinsResource(resourceManager);
            if (coins == null)
            {
                return;
            }

            coins.Add(50);
            Debug.Log("[DebugPanel] Added 50 coins.");
        }

        private void HandleSpendAllCoinsRequested()
        {
            var resourceManager = ResolveResourceManager();
            var coins = GetCoinsResource(resourceManager);
            if (coins == null)
            {
                return;
            }

            int currentCoins = coins.GetValue();
            if (currentCoins <= 0)
            {
                Debug.Log("[DebugPanel] No coins to spend.");
                return;
            }

            if (resourceManager != null)
            {
                if (!resourceManager.ConsumeWithEffects(coins, currentCoins))
                {
                    coins.Set(0);
                }
            }
            else
            {
                coins.Set(0);
            }

            Debug.Log($"[DebugPanel] Spent all coins ({currentCoins}).");
        }

        private void HandleInstantWinRequested()
        {
            var levelManager = FindObjectOfType<LevelManager>();
            if (levelManager == null)
            {
                Debug.LogWarning("[DebugPanel] Unable to find LevelManager for instant win request.");
                return;
            }

            levelManager.SetWin();
        }

        private void HandleCloseRequested()
        {
            Close();
        }

        public override void Close()
        {
            if (closeCompleted)
            {
                return;
            }

            base.Close();

            if (!HasHideAnimation())
            {
                CompleteCloseImmediately();
                return;
            }

            if (closeFallbackRoutine != null)
            {
                StopCoroutine(closeFallbackRoutine);
            }

            closeFallbackRoutine = StartCoroutine(CloseFallback());
        }

        public override void AfterHideAnimation()
        {
            if (closeCompleted)
            {
                return;
            }

            if (closeFallbackRoutine != null)
            {
                StopCoroutine(closeFallbackRoutine);
                closeFallbackRoutine = null;
            }

            closeCompleted = true;
            base.AfterHideAnimation();
        }

        private bool ValidateLevel(int levelNumber)
        {
            if (levelNumber <= 0)
            {
                Debug.LogWarning($"[DebugPanel] Invalid level number: {levelNumber}. Level number must be greater than zero.");
                return false;
            }

            bool levelExists = Resources.LoadAll<Level>("Levels").Any(level => level.number == levelNumber);
            if (!levelExists)
            {
                Debug.LogWarning($"[DebugPanel] Level with number {levelNumber} not found.");
                return false;
            }

            return true;
        }

        private void RefreshInputField()
        {
            if (levelInputField == null)
            {
                return;
            }

            int baseLevel = pendingLevelNumber ?? GameDataManager.GetLevelNum();
            levelInputField.text = baseLevel.ToString();
        }

        private void RefreshPendingLabel()
        {
            if (pendingLevelLabel == null)
            {
                return;
            }

            pendingLevelLabel.text = pendingLevelNumber.HasValue ? pendingLevelNumber.Value.ToString() : "-";
        }

        public static bool TryConsumePendingLevel(out int levelNumber)
        {
            if (pendingLevelNumber.HasValue)
            {
                levelNumber = pendingLevelNumber.Value;
                pendingLevelNumber = null;
                PendingLevelChanged?.Invoke();
                return true;
            }

            levelNumber = default;
            return false;
        }

        public static void NotifyExternalLevelChange()
        {
            PendingLevelChanged?.Invoke();
        }

        private bool HasHideAnimation()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                return false;
            }

            var controller = animator.runtimeAnimatorController;
            if (controller == null)
            {
                return false;
            }

            return controller.animationClips.Any(clip => clip != null && clip.name == "popup_hide");
        }

        private IEnumerator CloseFallback()
        {
            yield return new WaitForSeconds(1f);

            if (!closeCompleted)
            {
                CompleteCloseImmediately();
            }
        }

        private void CompleteCloseImmediately()
        {
            if (closeCompleted)
            {
                return;
            }

            if (closeFallbackRoutine != null)
            {
                StopCoroutine(closeFallbackRoutine);
                closeFallbackRoutine = null;
            }

            Hide();
            closeCompleted = true;
            base.AfterHideAnimation();
        }

        private T ResolveFromContainer<T>() where T : class
        {
            try
            {
                var scope = LifetimeScope.Find<GameLifetimeScope>();
                if (scope?.Container != null && scope.Container.TryResolve(out T resolved))
                {
                    return resolved;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DebugPanel] Failed to resolve {typeof(T).Name} from container: {ex.Message}");
            }

            return null;
        }

        private ResourceManager ResolveResourceManager()
        {
            var resourceManager = ResolveFromContainer<ResourceManager>();
            return resourceManager != null ? resourceManager : FindObjectOfType<ResourceManager>();
        }

        private ResourceObject GetCoinsResource(ResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                Debug.LogWarning("[DebugPanel] ResourceManager not found. Unable to modify coins.");
                return null;
            }

            var coinsResource = resourceManager.GetResource("Coins");
            if (coinsResource == null)
            {
                Debug.LogWarning("[DebugPanel] Coins resource not found.");
            }

            return coinsResource;
        }

    }
}
