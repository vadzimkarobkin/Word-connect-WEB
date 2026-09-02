using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;
using WordsToolkit.Scripts.Settings;

namespace WordsToolkit.Scripts.Levels
{
    [Serializable]
    public class LocalizedTextGroup
    {
        public string language;
        public string title;
        public string text;
    }

    [CreateAssetMenu(fileName = "NewLevelGroup", menuName = "WordConnectGameToolkit/Editor/Level Group")]
    public class LevelGroup : ScriptableObject
    {
        [Tooltip("Name of this group")]
        public string groupName;

        [Tooltip("Parent group (if any)")]
        [HideInInspector]
        public LevelGroup parentGroup;

        [Tooltip("Levels in this group")]
        public List<Level> levels = new List<Level>();
        
        [Tooltip("Background sprite for this group (Addressable reference)")]
        public AssetReference backgroundReference;

        [Tooltip("Language-specific text for this group")]
        public List<LocalizedTextGroup> localizedTexts = new List<LocalizedTextGroup>();

        [Tooltip("Colors tile for this group")]
        public ColorsTile colorsTile;

        [Tooltip("Target number of extra words for levels in this group")]
        public int targetExtraWords = 8;

        // Apply the group's colorsTile to all levels in this group
        public void ApplyColorsTileToLevels()
        {
            if (colorsTile == null || levels == null || levels.Count == 0)
                return;

            foreach (var level in levels)
            {
                if (level != null)
                {
                    level.colorsTile = this.colorsTile;
                }
            }
        }

        public string GetTitle(string languageCode)
        {
            var localizedText = GetGroupTextObject(languageCode);
            return localizedText != null ? localizedText.title : string.Empty;
        }

        public string GetText(string languageCode)
        {
            var localizedText = GetGroupTextObject(languageCode);
            return localizedText != null ? localizedText.text : string.Empty;
        }
        
        public AssetReference GetBackgroundReference()
        {
            if (backgroundReference != null && backgroundReference.RuntimeKeyIsValid())
            {
                return backgroundReference;
            }
            return null;
        }

        private LocalizedTextGroup GetGroupTextObject(string languageCode)
        {
            foreach (var localizedText in localizedTexts)
            {
                if (localizedText.language.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
                {
                    return localizedText;
                }
            }
            return null;
        }
        public void AddLanguage(string configLanguageCode)
        {
            localizedTexts.Add(new LocalizedTextGroup
            {
                language = configLanguageCode,
            });
        }
        
                public LevelGroup GetNextGroup()
        {
            var allGroups = Resources.LoadAll<LevelGroup>("Groups");

            if (allGroups == null || allGroups.Length == 0)
            {
                return null;
            }

            int currentFirstLevelNumber = GetFirstLevelNumber(this);
            LevelGroup nextGroup = null;
            int nextGroupFirstLevel = int.MaxValue;

            foreach (var group in allGroups)
            {
                if (group == null || group == this)
                {
                    continue;
                }

                int groupFirstLevel = GetFirstLevelNumber(group);

                if (groupFirstLevel <= 0)
                {
                    continue;
                }

                bool isCloserNextGroup = groupFirstLevel > currentFirstLevelNumber && groupFirstLevel < nextGroupFirstLevel;

                if (isCloserNextGroup)
                {
                    nextGroup = group;
                    nextGroupFirstLevel = groupFirstLevel;
                }
            }

            return nextGroup;
        }

        private static int GetFirstLevelNumber(LevelGroup group)
        {
            if (group?.levels == null || group.levels.Count == 0)
            {
                return -1;
            }

            return group.levels
                .Where(level => level != null)
                .Select(level => level.number)
                .DefaultIfEmpty(-1)
                .Min();
        }

    }
}
