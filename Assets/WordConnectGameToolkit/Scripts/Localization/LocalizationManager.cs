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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GamePush;
using UnityEngine;
using VContainer;
using WordsToolkit.Scripts.Levels;
using WordsToolkit.Scripts.Services;
using WordsToolkit.Scripts.Settings;
using WordsToolkit.Scripts.System;

namespace WordsToolkit.Scripts.Localization
{
    public class LocalizationManager : SingletonBehaviour<LocalizationManager>, ILocalizationService
    {
        private static DebugSettings _debugSettings;
        public static Dictionary<string, string> _dic;
        private Language _currentLanguage;
        [Inject]
        private LanguageConfiguration languageConfig;
        [Inject]
        private ILanguageService languageService;

        private Dictionary<string, string> _placeholdersDic;

        public override void Awake()
        {
            base.Awake();
            InitializeLocalization();
        }

        public void InitializeLocalization()
        {
            _debugSettings = Resources.Load("Settings/DebugSettings") as DebugSettings;
            LoadLanguage(GetSystemLanguage());
        }

        public void SetKeysPlaceholders(Dictionary<string, string> keys)
        {
            if (keys == null || keys.Count == 0)
            {
                Debug.LogWarning("Localization keys are null or empty. Using default language.");
                LoadLanguage(GP_Language.Current());
                return;
            }

            _placeholdersDic = new Dictionary<string, string>(keys);
        }

        public void SetPairPlaceholder(string key, string value)
        {
            _placeholdersDic ??= new Dictionary<string, string>();
            _placeholdersDic[key] = value;
        }

        public void LoadLanguage(Language language)
        {
            _currentLanguage = language;
                
            var fileName = language.ToString();
            var txt = Resources.Load<TextAsset>($"Localization/{fileName}");

            _dic = new Dictionary<string, string>();
            var lines = txt.text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var inp_ln in lines)
            {
                var l = inp_ln.Split(new[] { ':' }, 2);
                if (l.Length == 2)
                {
                    var key = l[0].Trim();
                    var text = l[1].Trim();
                    _dic[key] = text;
                }
            }
        }

        public void LoadLanguageFromBase(TextAsset localizationBase)
        {
            if (localizationBase == null)
            {
                Debug.LogWarning("Localization base is null. Falling back to English.");
                LoadLanguage(GP_Language.Current());
                return;
            }

            _dic = new Dictionary<string, string>();
            var lines = localizationBase.text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var inp_ln in lines)
            {
                var l = inp_ln.Split(new[] { ':' }, 2);
                if (l.Length == 2)
                {
                    var key = l[0].Trim();
                    var text = l[1].Trim();
                    _dic[key] = text;
                }
            }
        }

        public static Language GetSystemLanguage()
        {
            if (_debugSettings == null)
            {
                _debugSettings = Resources.Load("Settings/DebugSettings") as DebugSettings;
            }

            // Try to get language from LanguageService if available
            if (instance != null && instance.languageService != null)
            {
                try
                {
                    var languageCode = instance.languageService.GetCurrentLanguageCode();
                    return GP_Language.Current();
                }
                catch (Exception)
                {
                    // Fallback if there's an issue parsing the language code
                    Debug.LogWarning("Failed to parse language from LanguageService. Using fallback.");
                }
            }

            // Fallback to PlayerPrefs check for backward compatibility

            return GP_Language.Current();
        }

        public string GetText(string key, string defaultText)
        {
            var currentLanguage = GetSystemLanguage();
            if (_currentLanguage != currentLanguage || _dic == null || _dic.Count == 0)
            {
                LoadLanguage(currentLanguage);
            }

            if (!_dic.ContainsKey(key))
            {
                LoadLanguage(currentLanguage);
            }

            if (_dic.TryGetValue(key, out var localizedText) && !string.IsNullOrEmpty(localizedText))
            {
                return PlaceholderManager.ReplacePlaceholders(localizedText, _placeholdersDic);
            }

            return PlaceholderManager.ReplacePlaceholders(defaultText, _placeholdersDic);
        }

        public Language GetCurrentLanguage()
        {
            return _currentLanguage;
        }

        public string GetLocalizedCurrentLanguage()
        {
            if (languageService != null)
            {
                var currentLanguageInfo = languageService.GetCurrentLanguageInfo();
                if (currentLanguageInfo != null)
                {
                    return currentLanguageInfo.localizedName;
                }
            }

            // Fallback to old logic
            foreach (var lang in languageConfig.languages)
            {
                if (lang.displayName == _currentLanguage.ToString())
                {
                    return lang.localizedName;
                }
            }
            return _currentLanguage.ToString();
        }
    }
}