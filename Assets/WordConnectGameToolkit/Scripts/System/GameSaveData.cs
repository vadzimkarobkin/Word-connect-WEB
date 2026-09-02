using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace WordsToolkit.Scripts.System
{
    [Serializable]
    public class GameSaveData
    {
        [SerializeField] private Dictionary<string, string> stringData = new Dictionary<string, string>();
        [SerializeField] private Dictionary<string, float> numberData = new Dictionary<string, float>();
        [SerializeField] private Dictionary<string, bool> boolData = new Dictionary<string, bool>();

        // Методы для установки значений
        public void SetString(string key, string value)
        {
            if (stringData.ContainsKey(key))
                stringData[key] = value;
            else
                stringData.Add(key, value);
        }

        public void SetNumber(string key, float value)
        {
            if (numberData.ContainsKey(key))
                numberData[key] = value;
            else
                numberData.Add(key, value);
        }

        public void SetBool(string key, bool value)
        {
            if (boolData.ContainsKey(key))
                boolData[key] = value;
            else
                boolData.Add(key, value);
        }

        // Методы для получения значений
        public string GetString(string key, string defaultValue = "")
        {
            return stringData.ContainsKey(key) ? stringData[key] : defaultValue;
        }

        public float GetNumber(string key, float defaultValue = 0f)
        {
            return numberData.ContainsKey(key) ? numberData[key] : defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            return boolData.ContainsKey(key) ? boolData[key] : defaultValue;
        }

        // Проверка наличия ключа
        public bool HasKey(string key)
        {
            return stringData.ContainsKey(key) || numberData.ContainsKey(key) || boolData.ContainsKey(key);
        }

        // Удаление ключа
        public void RemoveKey(string key)
        {
            stringData.Remove(key);
            numberData.Remove(key);
            boolData.Remove(key);
        }

        // Очистка всех данных
        public void Clear()
        {
            stringData.Clear();
            numberData.Clear();
            boolData.Clear();
        }

        // Сериализация в JSON
        public string ToJson()
        {
            var wrapper = new SerializableGameSaveData
            {
                stringData = stringData,
                numberData = numberData,
                boolData = boolData
            };
            return JsonConvert.SerializeObject(wrapper);
        }

        // Десериализация из JSON
        public static GameSaveData FromJson(string json)
        {
            if (string.IsNullOrEmpty(json) || json == "0")
                return new GameSaveData();

            try
            {
                var wrapper = JsonConvert.DeserializeObject<SerializableGameSaveData>(json);
                var saveData = new GameSaveData();
                
                if (wrapper.stringData != null)
                    saveData.stringData = wrapper.stringData;
                if (wrapper.numberData != null)
                    saveData.numberData = wrapper.numberData;
                if (wrapper.boolData != null)
                    saveData.boolData = wrapper.boolData;

                return saveData;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize GameSaveData: {e.Message}");
                return new GameSaveData();
            }
        }
    }

    // Вспомогательный класс для сериализации Dictionary
    [Serializable]
    public class SerializableGameSaveData
    {
        public Dictionary<string, string> stringData;
        public Dictionary<string, float> numberData;
        public Dictionary<string, bool> boolData;
    }
}
