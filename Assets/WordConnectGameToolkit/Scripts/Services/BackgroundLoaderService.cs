using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using VContainer;
using WordsToolkit.Scripts.Levels;

namespace WordsToolkit.Scripts.Services
{
    public interface IBackgroundLoaderService
    {
        UniTask<Sprite> LoadBackgroundAsync(LevelGroup levelGroup);
        UniTask<Sprite> LoadBackgroundAsync(Level level);
        UniTask<Sprite> LoadBackgroundAsync(AssetReference assetReference);
        void ReleaseBackground(Sprite sprite);
        void ReleaseAllBackgrounds();
    }

    public class BackgroundLoaderService : IBackgroundLoaderService, IDisposable
    {
        private Dictionary<string, AsyncOperationHandle<Sprite>> loadedSprites =
            new Dictionary<string, AsyncOperationHandle<Sprite>>();

        private Dictionary<Sprite, string> spriteToKey = new Dictionary<Sprite, string>();

        public async UniTask<Sprite> LoadBackgroundAsync(LevelGroup levelGroup)
        {
            if (levelGroup == null)
            {
                Debug.LogWarning("[BackgroundLoaderService] LevelGroup is null");
                return null;
            }

            // Try AssetReference first
            var backgroundRef = levelGroup.GetBackgroundReference();
            if (backgroundRef != null)
            {
                return await LoadBackgroundAsync(backgroundRef);
            }

            Debug.LogWarning(
                $"[BackgroundLoaderService] No background reference found for LevelGroup: {levelGroup.groupName}");
            return null;
        }

        public async UniTask<Sprite> LoadBackgroundAsync(Level level)
        {
            if (level == null)
            {
                Debug.LogWarning("[BackgroundLoaderService] Level is null");
                return null;
            }

            // Try AssetReference first
            var backgroundRef = level.GetBackgroundReference();
            if (backgroundRef != null)
            {
                return await LoadBackgroundAsync(backgroundRef);
            }

            Debug.LogWarning($"[BackgroundLoaderService] No background reference found for Level: {level.number}");
            return null;
        }

        public async UniTask<Sprite> LoadBackgroundAsync(AssetReference assetReference)
        {
            Debug.Log($"[BackgroundLoaderService] LoadBackgroundAsync called with AssetReference: {assetReference?.RuntimeKey}");
            
            if (assetReference == null || !assetReference.RuntimeKeyIsValid())
            {
                Debug.LogWarning("[BackgroundLoaderService] AssetReference is null or invalid");
                return null;
            }

            string key = assetReference.RuntimeKey.ToString();
            Debug.Log($"[BackgroundLoaderService] Processing key: {key}");

            // Check if already loaded
            if (loadedSprites.ContainsKey(key))
            {
                Debug.Log($"[BackgroundLoaderService] Key {key} found in loadedSprites");
                var handle = loadedSprites[key];
                if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"[BackgroundLoaderService] Background already loaded: {key}");
                    return handle.Result;
                }
                else
                {
                    Debug.Log($"[BackgroundLoaderService] Handle invalid, removing key: {key}");
                    // Handle is invalid, remove it
                    loadedSprites.Remove(key);
                }
            }

            try
            {
                Debug.Log($"[BackgroundLoaderService] Starting Addressables.LoadAssetAsync for: {key}");
                var handle = Addressables.LoadAssetAsync<Sprite>(assetReference);
                loadedSprites[key] = handle;
                Debug.Log($"[BackgroundLoaderService] Handle stored in loadedSprites");

                Debug.Log($"[BackgroundLoaderService] Awaiting handle.ToUniTask()");
                var sprite = await handle.ToUniTask();
                Debug.Log($"[BackgroundLoaderService] ToUniTask completed, sprite: {sprite != null}");

                if (sprite != null)
                {
                    Debug.Log($"[BackgroundLoaderService] Sprite is not null");
                    Debug.Log($"[BackgroundLoaderService] Adding sprite to spriteToKey mapping");
                    spriteToKey[sprite] = key;
                    Debug.Log($"[BackgroundLoaderService] spriteToKey mapping added");
                    Debug.Log($"[BackgroundLoaderService] Successfully loaded background: {key}");
                    Debug.Log($"[BackgroundLoaderService] About to return sprite");
                    return sprite;
                }
                else
                {
                    Debug.LogError($"[BackgroundLoaderService] Failed to load background: {key}");
                    loadedSprites.Remove(key);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[BackgroundLoaderService] Exception in LoadBackgroundAsync for {key}: {e.Message}");
                Debug.LogError($"[BackgroundLoaderService] Stack trace: {e.StackTrace}");
                if (loadedSprites.ContainsKey(key))
                {
                    loadedSprites.Remove(key);
                }

                return null;
            }
        }

        public void ReleaseBackground(Sprite sprite)
        {
            if (sprite == null) return;

            if (spriteToKey.TryGetValue(sprite, out string key))
            {
                if (loadedSprites.TryGetValue(key, out var handle))
                {
                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                        Debug.Log($"[BackgroundLoaderService] Released background: {key}");
                    }

                    loadedSprites.Remove(key);
                }

                spriteToKey.Remove(sprite);
            }
        }

        public void ReleaseAllBackgrounds()
        {
            Debug.Log($"[BackgroundLoaderService] Releasing all {loadedSprites.Count} backgrounds");

            foreach (var kvp in loadedSprites)
            {
                if (kvp.Value.IsValid())
                {
                    Addressables.Release(kvp.Value);
                }
            }

            loadedSprites.Clear();
            spriteToKey.Clear();
        }

        public void Dispose()
        {
            
        }
    }
}