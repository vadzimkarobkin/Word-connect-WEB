using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using WordsToolkit.Scripts.Enums;
using WordsToolkit.Scripts.Levels;
using WordsToolkit.Scripts.Localization;
using WordsToolkit.Scripts.System;
using GamePush;

namespace WordsToolkit.Scripts.Services
{
    public interface ILanguageService
    {
        string GetCurrentLanguageCode();
        LanguageConfiguration.LanguageInfo GetCurrentLanguageInfo();
        List<LanguageConfiguration.LanguageInfo> GetEnabledLanguages();
        void SetLanguage(string languageCode);
        bool HasMultipleEnabledLanguages();
        void InitializeDefaultLanguage();
    }

    public class LanguageService : ILanguageService, IInitializable
    {
        private const string LANGUAGE_PREF_KEY = "SelectedLanguage";

        private static readonly List<string> _availableLanguages = new List<string>()
        {
            "en",
            "ru"
        };

        private readonly IObjectResolver container;
        private LanguageConfiguration languageConfiguration;
        private string currentLanguageCode;

        public LanguageService(IObjectResolver container)
        {
            this.container = container;
        }

        public void Initialize()
        {
            languageConfiguration = container.Resolve<LanguageConfiguration>();
            InitializeDefaultLanguage();
        }

        public void InitializeDefaultLanguage()
        {
            currentLanguageCode = GetSavedOrDefaultLanguage();
            
            // Only apply language changes in play mode
            #if UNITY_EDITOR
            if (Application.isPlaying)
            #endif
            {
                ApplyLanguage(currentLanguageCode);
            }
        }

        public string GetCurrentLanguageCode()
        {
            if (string.IsNullOrEmpty(currentLanguageCode))
            {
                currentLanguageCode = GetSavedOrDefaultLanguage();
            }
            return currentLanguageCode;
        }

        public LanguageConfiguration.LanguageInfo GetCurrentLanguageInfo()
        {
            if (languageConfiguration == null) return null;
            var languageCode = GetCurrentLanguageCode();
            return languageConfiguration.GetLanguageInfo(languageCode);
        }

        public List<LanguageConfiguration.LanguageInfo> GetEnabledLanguages()
        {
            if (languageConfiguration == null) return new List<LanguageConfiguration.LanguageInfo>();
            return languageConfiguration.GetEnabledLanguages();
        }

        public void SetLanguage(string languageCode)
        {
            if (languageConfiguration == null) return;
            var languageInfo = languageConfiguration.GetLanguageInfo(languageCode);
            if (languageInfo != null && languageInfo.enabledByDefault)
            {
                currentLanguageCode = languageCode;
                
                // Only save to GamePush in play mode
                #if UNITY_EDITOR
                if (Application.isPlaying)
                #endif
                {
                    try
                    {
                        GP_PlayerWrapper.Set(LANGUAGE_PREF_KEY, languageCode);
                    }
                    catch (SystemException ex)
                    {
                        Debug.LogWarning($"LanguageService: Failed to save language to GP: {ex.Message}");
                    }
                }
                
                ApplyLanguage(languageCode);
                
                #if UNITY_EDITOR
                if (Application.isPlaying)
                #endif
                {
                    try
                    {
                        EventManager.GetEvent<string>(EGameEvent.LanguageChanged).Invoke(languageCode);
                    }
                    catch (SystemException ex)
                    {
                        Debug.LogWarning($"LanguageService: Failed to invoke language change event: {ex.Message}");
                    }
                }
            }
        }

        public bool HasMultipleEnabledLanguages()
        {
            return GetEnabledLanguages().Count > 1;
        }

        private string GetSavedOrDefaultLanguage()
        {
            // In Editor mode, always use default language from configuration
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (languageConfiguration == null)
                {
                    Debug.LogWarning("LanguageService [Editor]: LanguageConfiguration is null, using 'en'");
                    return "ru";
                }
                
                var defaultLang = languageConfiguration.defaultLanguage ?? "ru";
                Debug.Log($"LanguageService [Editor]: Using default language: {defaultLang}");
                return defaultLang;
            }
            #endif
            
            if (languageConfiguration == null) 
            {
                Debug.LogWarning("LanguageService: LanguageConfiguration is null, using 'ru'");
                try
                {
                    var gpLanguage = GP_Language.CurrentISO();
                    Debug.LogWarning("GP LANGUAGE: " + gpLanguage);

                    if (!_availableLanguages.Contains(gpLanguage))
                    {
                        GP_Language.Change(Language.Russian);

                        return "ru";
                    }
                    
                    return !string.IsNullOrEmpty(gpLanguage) ? gpLanguage : "ru";
                }
                catch (SystemException ex)
                {
                    Debug.LogWarning($"LanguageService: Failed to get GP language: {ex.Message}. Using 'ru'");
                    return "ru";
                }
            }
            
            var enabledLanguages = languageConfiguration.GetEnabledLanguages();
            Debug.Log($"LanguageService: Found {enabledLanguages.Count} enabled languages");
            
            // If only one language is enabled, use it regardless of saved preference
            if (enabledLanguages.Count == 1)
            {
                var singleLanguage = enabledLanguages[0].code;
                Debug.Log($"LanguageService: Using single enabled language: {singleLanguage}");
                try
                {
                    var gpLanguage = GP_Language.CurrentISO();
                    Debug.LogWarning("GP LANGUAGE: " + gpLanguage);
                    
                    if (!_availableLanguages.Contains(gpLanguage))
                    {
                        GP_Language.Change(Language.Russian);

                        return "ru";
                    }
                    
                    return !string.IsNullOrEmpty(gpLanguage) ? gpLanguage : singleLanguage;
                }
                catch (SystemException ex)
                {
                    Debug.LogWarning($"LanguageService: Failed to get GP language: {ex.Message}. Using {singleLanguage}");
                    return singleLanguage;
                }
            }
            
            // If multiple languages are enabled, use saved preference or default
            var defaultLanguage = languageConfiguration.defaultLanguage ?? "ru";
            try
            {
                var selectedLang = GP_Language.CurrentISO();
                Debug.Log($"LanguageService: Using selected/default language: {selectedLang} (default: {defaultLanguage})");
                Debug.LogWarning("GP LANGUAGE: " + selectedLang);
                
                if (!_availableLanguages.Contains(selectedLang))
                {
                    GP_Language.Change(Language.Russian);

                    return "ru";
                }
                
                return !string.IsNullOrEmpty(selectedLang) ? selectedLang : defaultLanguage;
            }
            catch (SystemException ex)
            {
                Debug.LogWarning($"LanguageService: Failed to get GP language: {ex.Message}. Using default: {defaultLanguage}");
                return defaultLanguage;
            }
        }

        private void ApplyLanguage(string languageCode)
        {
            if (languageConfiguration == null) return;
            var languageInfo = languageConfiguration.GetLanguageInfo(languageCode);
            if (languageInfo?.localizationBase != null)
            {
                // Check if LocalizationManager instance exists before using it
                if (LocalizationManager.instance != null)
                {
                    LocalizationManager.instance.LoadLanguageFromBase(languageInfo.localizationBase);
                }
                #if UNITY_EDITOR
                else if (!Application.isPlaying)
                {
                    Debug.Log($"LanguageService [Editor]: LocalizationManager not available, skipping language application for {languageCode}");
                }
                #endif
            }
        }
    }
}
