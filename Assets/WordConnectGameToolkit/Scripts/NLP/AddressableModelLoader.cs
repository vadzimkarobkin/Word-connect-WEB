using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace WordsToolkit.Scripts.NLP
{
    /// <summary>
    /// Handles loading of ML models through Addressables system
    /// </summary>
    public class AddressableModelLoader : IDisposable
    {
        private Dictionary<string, ModelAsset> loadedModels = new Dictionary<string, ModelAsset>();
        private Dictionary<string, AsyncOperationHandle<ModelAsset>> loadingHandles = new Dictionary<string, AsyncOperationHandle<ModelAsset>>();
        
        public async UniTask<byte[]> LoadModelDataAsync(string addressableKey, string language, Action<float> onProgress = null)
        {
            try
            {
                Debug.Log($"[AddressableModelLoader] Loading model data for language: {language} with key: {addressableKey}");
                
                var handle = Addressables.LoadAssetAsync<TextAsset>(addressableKey);
                
                // Track progress
                while (!handle.IsDone)
                {
                    onProgress?.Invoke(handle.PercentComplete);
                    await UniTask.Yield();
                }
                
                onProgress?.Invoke(1f);
                
                var textAsset = await handle.ToUniTask();
                
                if (textAsset != null)
                {
                    Debug.Log($"[AddressableModelLoader] Successfully loaded model data for language: {language} (size: {textAsset.bytes.Length} bytes)");
                    return textAsset.bytes;
                }
                else
                {
                    Debug.LogError($"[AddressableModelLoader] Failed to load TextAsset for language: {language}");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressableModelLoader] Error loading model data for language {language}: {e.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            // Release all loaded models
            foreach (var handle in loadingHandles.Values)
            {
                Addressables.Release(handle);
            }
            
            loadedModels.Clear();
            loadingHandles.Clear();
        }
    }
}