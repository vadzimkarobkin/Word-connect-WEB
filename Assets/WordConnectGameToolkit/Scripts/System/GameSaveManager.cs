using UnityEngine;
using System;
using GamePush;

namespace WordsToolkit.Scripts.System
{
    public class GameSaveManager : MonoBehaviour
    {
        private const string SAVE_KEY = "save";
        private static GameSaveManager _instance;
        private static GameSaveData _gameSaveData;

        public static GameSaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameSaveManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameSaveManager");
                        _instance = go.AddComponent<GameSaveManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadGameData();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        // Загрузка данных из PlayerPrefs
        public static void LoadGameData()
        {
            string jsonData = GP_Player.GetString(SAVE_KEY);
            _gameSaveData = GameSaveData.FromJson(jsonData);
            
            if (_gameSaveData == null)
            {
                _gameSaveData = new GameSaveData();
            }
            
            Debug.Log("Game data loaded from PlayerPrefs");
        }

        // Сохранение данных в PlayerPrefs
        public static void SaveGameData()
        {
            if (_gameSaveData != null)
            {
                string jsonData = _gameSaveData.ToJson();
                GP_Player.Set(SAVE_KEY, jsonData);
                GP_Player.Sync();
                Debug.Log("Game data saved");
            }
        }

        // Получение экземпляра GameSaveData
        public static GameSaveData GetGameSaveData()
        {
            if (_gameSaveData == null)
            {
                LoadGameData();
            }
            return _gameSaveData;
        }

        // Автоматическое сохранение при изменении данных
        public static void MarkDataChanged()
        {
            SaveGameData();
        }

        // Очистка всех сохранений
        public static void ClearAllSaves()
        {
            _gameSaveData = new GameSaveData();
            GP_Player.ResetPlayer();
            Debug.Log("All game data cleared");
        }

        // Загрузка данных из JSON
        public static void LoadFromJson(string jsonData)
        {
            _gameSaveData = GameSaveData.FromJson(jsonData);
            if (_gameSaveData == null)
            {
                _gameSaveData = new GameSaveData();
            }
            SaveGameData(); // Сохраняем загруженные данные
            Debug.Log("Game data loaded from JSON");
        }

        // Принудительное сохранение при выходе из приложения
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGameData();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveGameData();
            }
        }

        private void OnDestroy()
        {
            SaveGameData();
        }
    }
}
