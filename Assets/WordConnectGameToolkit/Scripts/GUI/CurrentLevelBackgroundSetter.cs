using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using WordsToolkit.Scripts.Levels;
using WordsToolkit.Scripts.Services;
using WordsToolkit.Scripts.System;

namespace WordsToolkit.Scripts.GUI
{
    /// <summary>
    /// Sets the provided <see cref="Image"/> component's sprite to the background of the currently active level.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CurrentLevelBackgroundSetter : MonoBehaviour
    {
        [SerializeField]
        private Image targetImage;

        private IBackgroundLoaderService backgroundLoaderService;
        private CancellationTokenSource cancellationTokenSource;

        [Inject]
        public void Construct(IBackgroundLoaderService backgroundLoaderService)
        {
            this.backgroundLoaderService = backgroundLoaderService;
        }

        private void Awake()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }
        }

        private void OnEnable()
        {
            cancellationTokenSource = new CancellationTokenSource();
            SetBackgroundAsync(cancellationTokenSource.Token).Forget();
        }

        private void OnDisable()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        private async UniTask SetBackgroundAsync(CancellationToken token)
        {
            if (targetImage == null)
            {
                Debug.LogError("[CurrentLevelBackgroundSetter] Target Image is not assigned.");
                return;
            }

            var level = GameDataManager.GetLevel();
            if (level == null)
            {
                Debug.LogWarning("[CurrentLevelBackgroundSetter] Current level is not available.");
                return;
            }

            var background = await LoadBackgroundAsync(level, token);
            if (background == null)
            {
                Debug.LogWarning($"[CurrentLevelBackgroundSetter] Failed to load background for level: {level.number}");
                return;
            }

            if (token.IsCancellationRequested || this == null)
            {
                return;
            }

            targetImage.sprite = background;
        }

        private async UniTask<Sprite> LoadBackgroundAsync(Level level, CancellationToken token)
        {
            if (backgroundLoaderService == null)
            {
                Debug.LogError("[CurrentLevelBackgroundSetter] BackgroundLoaderService is not available.");
                return null;
            }

            try
            {
                return await backgroundLoaderService
                    .LoadBackgroundAsync(level)
                    .AttachExternalCancellation(token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
    }
}
