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
using System.Threading;
using Cysharp.Threading.Tasks;
using GamePush;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using WordsToolkit.Scripts.Gameplay.Managers;
using WordsToolkit.Scripts.NLP;
using WordsToolkit.Scripts.GUI;
using WordsToolkit.Scripts.Levels;
using WordsToolkit.Scripts.Services;
using WordsToolkit.Scripts.System;

namespace WordsToolkit.Scripts.Popups
{
    public class Loading : Popup
    {
        [SerializeField]
        private Image progressFillImage;

        [SerializeField]
        private Slider progressSlider;

        [SerializeField]
        private TextMeshProUGUI progressText;

        [SerializeField, Min(0f)]
        private float fadeHideDelay = 0.5f;

        [SerializeField, Min(0f)]
        private float fadeOutDuration = 0.5f;

        [SerializeField, Min(0f)]
        private float backgroundPreparationFallbackDelay = 0.25f;

        [SerializeField]
        private Image fadeImage;

        [SerializeField]
        private CanvasGroup progressCanvasGroup;

        private IModelController modelController;
        private SceneLoader sceneLoader;
        private bool isSubscribedToProgress;
        private Coroutine fadeRoutine;
        private Color fadeInitialColor;
        private bool fadeInitialColorCaptured;
        private float progressInitialAlpha = 1f;
        private bool hasTriggeredLevelStart;
        private bool completionSequenceStarted;
        private CancellationTokenSource completionSequenceCts;
        private IBackgroundLoaderService backgroundLoaderService;
        private BackgroundChanger backgroundChanger;
        private bool backgroundLoaderResolveAttempted;
        private bool backgroundChangerResolveAttempted;

        [Inject]
        public void ConstructLoading(IModelController modelController, SceneLoader sceneLoader)
        {
            this.modelController = modelController;
            this.sceneLoader = sceneLoader;
        }

        protected override void Awake()
        {
            base.Awake();
            EnsureMainScreenState();
            InitializeFadeReferences();
            InitializeProgressCanvasGroup();
            EnsureFadeVisible();
            UpdateProgressUI(0f);
        }

        private void OnEnable()
        {
            ModelController.OnInstanceCreated += HandleModelControllerCreated;
            ModelController.OnInstanceDisposed += HandleModelControllerDisposed;

            CancelFadeRoutine();
            EnsureFadeVisible();
            EnsureMainScreenState();

            if (!EnsureModelController())
            {
                UpdateProgressUI(0f);
                return;
            }

            SubscribeToModelController();
        }

        private void OnDisable()
        {
            ModelController.OnInstanceCreated -= HandleModelControllerCreated;
            ModelController.OnInstanceDisposed -= HandleModelControllerDisposed;
            UnsubscribeFromModelController();
            CancelFadeRoutine();
            CancelCompletionSequence();
        }

        public override void CloseAnimationSound()
        {
        }

        public override void Hide()
        {
        }

        private void HandleOverallProgressChanged(float progress)
        {
            UpdateProgressUI(progress);
        }

        private bool EnsureModelController()
        {
            if (modelController != null)
            {
                return true;
            }

            modelController = ModelController.ActiveInstance;

            if (modelController == null && _container != null)
            {
                try
                {
                    modelController = _container.Resolve<IModelController>();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[Loading] Failed to resolve IModelController: {exception.Message}");
                }
            }

            return modelController != null;
        }

        private void HandleModelControllerCreated(IModelController controller)
        {
            if (controller == null || controller == modelController)
            {
                return;
            }

            UnsubscribeFromModelController();
            modelController = controller;

            if (isActiveAndEnabled)
            {
                SubscribeToModelController();
            }
        }

        private void HandleModelControllerDisposed(IModelController controller)
        {
            if (controller != null && controller == modelController)
            {
                UnsubscribeFromModelController();
                modelController = null;
                UpdateProgressUI(0f);
            }
        }

        private void SubscribeToModelController()
        {
            if (modelController == null || isSubscribedToProgress)
            {
                return;
            }

            modelController.OnOverallModelLoadProgressChanged += HandleOverallProgressChanged;
            isSubscribedToProgress = true;
            HandleOverallProgressChanged(modelController.OverallModelLoadProgress);
        }

        private void UnsubscribeFromModelController()
        {
            if (modelController != null && isSubscribedToProgress)
            {
                modelController.OnOverallModelLoadProgressChanged -= HandleOverallProgressChanged;
            }

            isSubscribedToProgress = false;
        }

        private void UpdateProgressUI(float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (progressFillImage != null)
            {
                progressFillImage.fillAmount = progress;
            }

            if (progressSlider != null)
            {
                progressSlider.normalizedValue = progress;
            }

            if (progressText != null)
            {
                progressText.text = $"{Mathf.RoundToInt(progress * 100f)}%";
            }

            if (progress < 1f)
            {
                CancelFadeRoutine();
                EnsureFadeVisible();
                CancelCompletionSequence();
            }
            else if (!completionSequenceStarted)
            {
                BeginCompletionSequence();
            }
        }

        private void BeginCompletionSequence()
        {
            if (completionSequenceStarted)
            {
                return;
            }

            CancelCompletionSequenceInternal();
            completionSequenceStarted = true;
            completionSequenceCts = new CancellationTokenSource();
            CompleteLoadingAsync(completionSequenceCts.Token).Forget();
        }

        private void CancelCompletionSequence()
        {
            completionSequenceStarted = false;
            CancelCompletionSequenceInternal();
        }

        private void CancelCompletionSequenceInternal()
        {
            if (completionSequenceCts != null)
            {
                completionSequenceCts.Cancel();
                completionSequenceCts.Dispose();
                completionSequenceCts = null;
            }
        }

        private async UniTaskVoid CompleteLoadingAsync(CancellationToken cancellationToken)
        {
            try
            {
                await PrepareBackgroundAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Loading] Background preparation failed: {exception.Message}");
            }

            if (cancellationToken.IsCancellationRequested || this == null)
            {
                return;
            }

            TriggerLevelStart();

            if (fadeRoutine == null)
            {
                fadeRoutine = StartCoroutine(FadeOutAfterDelay());
            }
        }

        private async UniTask PrepareBackgroundAsync(CancellationToken cancellationToken)
        {
            var backgroundPrepared = false;

            try
            {
                backgroundPrepared = await TryPreloadBackgroundAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }

            if (!backgroundPrepared && backgroundPreparationFallbackDelay > 0f)
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(backgroundPreparationFallbackDelay),
                    DelayType.Realtime,
                    PlayerLoopTiming.Update,
                    cancellationToken);
            }
        }

        private async UniTask<bool> TryPreloadBackgroundAsync(CancellationToken cancellationToken)
        {
            var loaderService = EnsureBackgroundLoaderService();

            if (loaderService == null)
            {
                return false;
            }

            Level level = GameDataManager.GetLevel();

            if (level == null)
            {
                return false;
            }

            Sprite backgroundSprite;

            try
            {
                backgroundSprite = await loaderService
                    .LoadBackgroundAsync(level)
                    .AttachExternalCancellation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Loading] Failed to preload background: {exception.Message}");
                return false;
            }

            if (cancellationToken.IsCancellationRequested || backgroundSprite == null)
            {
                return backgroundSprite != null;
            }

            var changer = EnsureBackgroundChanger();

            if (changer == null)
            {
                return false;
            }

            changer.SetBackground(backgroundSprite);
            return true;
        }

        private IBackgroundLoaderService EnsureBackgroundLoaderService()
        {
            if (backgroundLoaderService != null)
            {
                return backgroundLoaderService;
            }

            if (!backgroundLoaderResolveAttempted && _container != null)
            {
                backgroundLoaderResolveAttempted = true;

                try
                {
                    backgroundLoaderService = _container.Resolve<IBackgroundLoaderService>();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[Loading] Failed to resolve IBackgroundLoaderService: {exception.Message}");
                }
            }

            return backgroundLoaderService;
        }

        private BackgroundChanger EnsureBackgroundChanger()
        {
            if (backgroundChanger != null)
            {
                return backgroundChanger;
            }

            if (!backgroundChangerResolveAttempted && _container != null)
            {
                backgroundChangerResolveAttempted = true;

                try
                {
                    backgroundChanger = _container.Resolve<BackgroundChanger>();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[Loading] Failed to resolve BackgroundChanger: {exception.Message}");
                }
            }

            if (backgroundChanger == null)
            {
                backgroundChanger = FindObjectOfType<BackgroundChanger>();
            }

            return backgroundChanger;
        }

        private IEnumerator FadeOutAfterDelay()
        {
            if (fadeHideDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(fadeHideDelay);
            }

            if (fadeOutDuration <= 0f)
            {
                ApplyFadeAlpha(0f);
                FinalizeFadeOut();
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var normalizedTime = Mathf.Clamp01(elapsed / fadeOutDuration);
                var alpha = Mathf.Lerp(1f, 0f, normalizedTime);
                ApplyFadeAlpha(alpha);
                yield return null;
            }

            ApplyFadeAlpha(0f);
            FinalizeFadeOut();
        }

        private void InitializeFadeReferences()
        {
            if (fadeImage == null)
            {
                fadeImage = FindFadeImage();
            }

            if (fadeImage != null)
            {
                fadeInitialColor = fadeImage.color;
                fadeInitialColorCaptured = true;
            }
            else
            {
                fadeInitialColor = new Color(0f, 0f, 0f, 1f);
                fadeInitialColorCaptured = false;
            }
        }

        private void InitializeProgressCanvasGroup()
        {
            if (progressCanvasGroup == null)
            {
                progressCanvasGroup = GetComponent<CanvasGroup>();
            }

            if (progressCanvasGroup != null)
            {
                progressInitialAlpha = progressCanvasGroup.alpha;
            }
        }

        private void EnsureFadeVisible()
        {
            if (fadeImage == null)
            {
                fadeImage = FindFadeImage();
            }

            if (fadeImage != null)
            {
                if (!fadeInitialColorCaptured)
                {
                    fadeInitialColor = fadeImage.color;
                    fadeInitialColorCaptured = true;
                }

                var fadeGameObject = fadeImage.gameObject;
                if (!fadeGameObject.activeSelf)
                {
                    fadeGameObject.SetActive(true);
                }

                fadeImage.raycastTarget = true;
                SetFadeAlpha(1f);
            }

            SetProgressVisible(true);
            SetProgressAlpha(1f);

            if (progressCanvasGroup != null)
            {
                progressCanvasGroup.interactable = true;
                progressCanvasGroup.blocksRaycasts = true;
            }
        }

        private void ApplyFadeAlpha(float alpha)
        {
            SetFadeAlpha(alpha);
            SetProgressAlpha(alpha);
        }

        private void SetFadeAlpha(float normalizedAlpha)
        {
            if (fadeImage == null)
            {
                return;
            }

            var clampedAlpha = Mathf.Clamp01(normalizedAlpha);
            var color = fadeInitialColor;
            color.a = clampedAlpha * fadeInitialColor.a * Mathf.Clamp01(fadeAlpha);
            fadeImage.color = color;
        }

        private void SetProgressAlpha(float normalizedAlpha)
        {
            if (progressCanvasGroup != null)
            {
                progressCanvasGroup.alpha = Mathf.Clamp01(normalizedAlpha) * progressInitialAlpha;
                return;
            }

            if (progressFillImage != null)
            {
                SetGraphicAlpha(progressFillImage, normalizedAlpha);
            }

            if (progressText != null)
            {
                SetTextAlpha(progressText, normalizedAlpha);
            }

            if (progressSlider != null)
            {
                var targetGraphic = progressSlider.targetGraphic;
                if (targetGraphic != null)
                {
                    SetGraphicAlpha(targetGraphic, normalizedAlpha);
                }
            }
        }

        private static void SetGraphicAlpha(Graphic graphic, float normalizedAlpha)
        {
            if (graphic == null)
            {
                return;
            }

            var color = graphic.color;
            color.a = Mathf.Clamp01(normalizedAlpha);
            graphic.color = color;
        }

        private static void SetTextAlpha(TMP_Text text, float normalizedAlpha)
        {
            if (text == null)
            {
                return;
            }

            var color = text.color;
            color.a = Mathf.Clamp01(normalizedAlpha);
            text.color = color;
        }

        private void FinalizeFadeOut()
        {
            if (fadeImage != null)
            {
                fadeImage.raycastTarget = false;
                var fadeGameObject = fadeImage.gameObject;
                if (fadeGameObject.activeSelf)
                {
                    fadeGameObject.SetActive(false);
                }
            }

            SetProgressVisible(false);

            if (progressCanvasGroup != null)
            {
                progressCanvasGroup.interactable = false;
                progressCanvasGroup.blocksRaycasts = false;
            }

            fadeRoutine = null;
            GP_Game.GameReady();
        }

        private void CancelFadeRoutine()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }

            if (fadeImage != null)
            {
                fadeImage.raycastTarget = true;
            }
        }

        private void EnsureMainScreenState()
        {
            if (stateManager == null || hasTriggeredLevelStart)
            {
                return;
            }

            if (stateManager.CurrentState != EScreenStates.MainMenu)
            {
                stateManager.CurrentState = EScreenStates.MainMenu;
            }
        }

        private void TriggerLevelStart()
        {
            if (hasTriggeredLevelStart)
            {
                return;
            }

            hasTriggeredLevelStart = true;

            if (sceneLoader == null && _container != null)
            {
                try
                {
                    sceneLoader = _container.Resolve<SceneLoader>();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[Loading] Failed to resolve SceneLoader: {exception.Message}");
                }
            }

            if (sceneLoader != null)
            {
                sceneLoader.StartGameScene();
            }
            else
            {
                Debug.LogWarning("[Loading] SceneLoader reference is missing. Unable to start game scene after loading.");
            }
        }

        private void SetProgressVisible(bool visible)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.gameObject.activeSelf != visible)
                {
                    child.gameObject.SetActive(visible);
                }
            }

            ToggleGameObject(progressSlider != null ? progressSlider.gameObject : null, visible);
            ToggleGameObject(progressFillImage != null ? progressFillImage.gameObject : null, visible);
            ToggleGameObject(progressText != null ? progressText.gameObject : null, visible);
        }

        private static void ToggleGameObject(GameObject target, bool visible)
        {
            if (target != null && target.activeSelf != visible)
            {
                target.SetActive(visible);
            }
        }

        private Image FindFadeImage()
        {
            var parent = transform.parent;
            if (parent == null)
            {
                return null;
            }

            Image fallback = null;
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child == transform)
                {
                    continue;
                }

                var candidate = child.GetComponent<Image>();
                if (candidate == null)
                {
                    continue;
                }

                if (string.Equals(child.name, "Fade", StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }

                if (fallback == null)
                {
                    fallback = candidate;
                }
            }

            return fallback;
        }
    }
}
