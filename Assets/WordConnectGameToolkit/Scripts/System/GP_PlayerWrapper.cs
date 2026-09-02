using UnityEngine;
using GamePush;
using WordsToolkit.Scripts.System;

namespace WordsToolkit.Scripts.System
{
    /// <summary>
    /// Обертка для GP_Player, которая перехватывает все вызовы Set и сохраняет их в GameSaveData
    /// </summary>
    public static class GP_PlayerWrapper
    {
        private static bool _isInitialized = false;

        // Инициализация обертки
        public static void Initialize()
        {
            if (!_isInitialized)
            {
                // Убеждаемся, что GameSaveManager инициализирован
                GameSaveManager.Instance.ToString(); // Принудительная инициализация
                _isInitialized = true;
                Debug.Log("GP_PlayerWrapper initialized");
            }
        }

        // Перехваченные методы Set
        public static void Set(string key, string value)
        {
            Initialize();
            
            // Сохраняем в нашу обертку
            var saveData = GameSaveManager.GetGameSaveData();
            saveData.SetString(key, value);
            GameSaveManager.MarkDataChanged();
            
            // Также сохраняем в оригинальный GP_Player для совместимости
           // GP_Player.Set(key, value);
            
            Debug.Log($"GP_PlayerWrapper.Set: {key} = {value} (string)");
        }

        public static void Set(string key, int value)
        {
            Initialize();
            
            // Сохраняем в нашу обертку
            var saveData = GameSaveManager.GetGameSaveData();
            saveData.SetNumber(key, value);
            GameSaveManager.MarkDataChanged();
            
            // Также сохраняем в оригинальный GP_Player для совместимости
            //GP_Player.Set(key, value);
            
            Debug.Log($"GP_PlayerWrapper.Set: {key} = {value} (int)");
        }

        public static void Set(string key, float value)
        {
            Initialize();
            
            // Сохраняем в нашу обертку
            var saveData = GameSaveManager.GetGameSaveData();
            saveData.SetNumber(key, value);
            GameSaveManager.MarkDataChanged();
            
            // Также сохраняем в оригинальный GP_Player для совместимости
            //GP_Player.Set(key, value);
            
            Debug.Log($"GP_PlayerWrapper.Set: {key} = {value} (float)");
        }

        public static void Set(string key, bool value)
        {
            Initialize();
            
            // Сохраняем в нашу обертку
            var saveData = GameSaveManager.GetGameSaveData();
            saveData.SetBool(key, value);
            GameSaveManager.MarkDataChanged();
            
            // Также сохраняем в оригинальный GP_Player для совместимости
            //GP_Player.Set(key, value);
            
            Debug.Log($"GP_PlayerWrapper.Set: {key} = {value} (bool)");
        }

        // Перехваченные методы Get - сначала проверяем нашу обертку, потом оригинальный GP_Player
        public static string GetString(string key, string defaultValue = "")
        {
            Initialize();
            
            var saveData = GameSaveManager.GetGameSaveData();
            if (saveData.HasKey(key))
            {
                string value = saveData.GetString(key, defaultValue);
                Debug.Log($"GP_PlayerWrapper.GetString: {key} = {value} (from wrapper)");
                return value;
            }

            return defaultValue;
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            Initialize();
            
            var saveData = GameSaveManager.GetGameSaveData();
            if (saveData.HasKey(key))
            {
                int value = (int)saveData.GetNumber(key, defaultValue);
                Debug.Log($"GP_PlayerWrapper.GetInt: {key} = {value} (from wrapper)");
                return value;
            }

            return defaultValue;

            // Fallback к оригинальному GP_Player
            // int originalValue = GP_Player.GetInt(key);
            //Debug.Log($"GP_PlayerWrapper.GetInt: {key} = {originalValue} (from GP_Player)");
            // return originalValue;
        }

        public static float GetFloat(string key, float defaultValue = 0f)
        {
            Initialize();
            
            var saveData = GameSaveManager.GetGameSaveData();
            if (saveData.HasKey(key))
            {
                float value = saveData.GetNumber(key, defaultValue);
                Debug.Log($"GP_PlayerWrapper.GetFloat: {key} = {value} (from wrapper)");
                return value;
            }

            return defaultValue;
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            Initialize();
            
            var saveData = GameSaveManager.GetGameSaveData();
            if (saveData.HasKey(key))
            {
                bool value = saveData.GetBool(key, defaultValue);
                Debug.Log($"GP_PlayerWrapper.GetBool: {key} = {value} (from wrapper)");
                return value;
            }

            return defaultValue;
        }

        // Проверка наличия ключа
        public static bool Has(string key)
        {
            Initialize();
            
            var saveData = GameSaveManager.GetGameSaveData();
            return saveData.HasKey(key);
        }

        // Дополнительные методы для работы с сохранениями
        public static void SaveGame()
        {
            Initialize();
            GameSaveManager.SaveGameData();
            Debug.Log("Game saved through GP_PlayerWrapper");
        }

        public static void LoadGame()
        {
            Initialize();
            GameSaveManager.LoadGameData();
            Debug.Log("Game loaded through GP_PlayerWrapper");
        }

        public static void ClearAllData()
        {
            Initialize();
            GameSaveManager.ClearAllSaves();
            Debug.Log("All data cleared through GP_PlayerWrapper");
        }

        // Получение JSON представления всех сохранений
        public static string GetSaveDataAsJson()
        {
            Initialize();
            var saveData = GameSaveManager.GetGameSaveData();
            return saveData.ToJson();
        }

        // Загрузка данных из JSON
        public static void LoadSaveDataFromJson(string jsonData)
        {
            Initialize();
            GameSaveManager.LoadFromJson(jsonData);
            Debug.Log("Save data loaded from JSON through GP_PlayerWrapper");
        }
    }
}
