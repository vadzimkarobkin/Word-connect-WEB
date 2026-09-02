using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace WordConnectGameToolkit.Scripts
{
    /// <summary>
    /// Handles loading of ML models through Addressables system
    /// </summary>
    public class AddressableModelLoader : IDisposable
    {
        private Dictionary<string, ModelAsset> loadedModels = new Dictionary<string, ModelAsset>();
        private Dictionary<string, AsyncOperationHandle<ModelAsset>> loadingHandles = new Dictionary<string, AsyncOperationHandle<ModelAsset>>();

        /// <summary>
        /// Loads a model asset asynchronously using Addressables
        /// </summary>
        /// <param name="addressableKey">The addressable key for the model</param>
        /// <param name="language">Language code for caching</param>
        /// <returns>Task that completes when model is loaded</returns>
        public async Task<ModelAsset> LoadModelAsync(AssetReference addressableKey, string language)
        {
            // Check if already loaded
            if (loadedModels.ContainsKey(language))
            {
                return loadedModels[language];
            }

            // Check if currently loading
            if (loadingHandles.ContainsKey(language))
            {
                await loadingHandles[language].Task;
                return loadedModels[language];
            }

            try
            {
                Debug.Log($"[AddressableModelLoader] Loading model for language: {language} with key: {addressableKey}");
                
                // Load the model asset asynchronously
                var handle = Addressables.LoadAssetAsync<ModelAsset>(addressableKey);
                loadingHandles[language] = handle;
                
                var modelAsset = await handle.Task;
                
                if (modelAsset != null)
                {
                    loadedModels[language] = modelAsset;
                    Debug.Log($"[AddressableModelLoader] Successfully loaded model for language: {language}");
                }
                else
                {
                    Debug.LogError($"[AddressableModelLoader] Failed to load model for language: {language}");
                }

                return modelAsset;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressableModelLoader] Error loading model for language {language}: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Unloads a specific model
        /// </summary>
        /// <param name="language">Language code to unload</param>
        public void UnloadModel(string language)
        {
            if (loadedModels.ContainsKey(language))
            {
                loadedModels.Remove(language);
            }

            if (loadingHandles.ContainsKey(language))
            {
                Addressables.Release(loadingHandles[language]);
                loadingHandles.Remove(language);
            }
        }

        /// <summary>
        /// Checks if a model is loaded
        /// </summary>
        /// <param name="language">Language code to check</param>
        /// <returns>True if model is loaded</returns>
        public bool IsModelLoaded(string language)
        {
            return loadedModels.ContainsKey(language);
        }

        /// <summary>
        /// Gets a loaded model
        /// </summary>
        /// <param name="language">Language code</param>
        /// <returns>Loaded model asset or null</returns>
        public ModelAsset GetLoadedModel(string language)
        {
            return loadedModels.TryGetValue(language, out var model) ? model : null;
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