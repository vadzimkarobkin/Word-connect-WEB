using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WordsToolkit.Scripts.System;

public class CellsTextMapper : MonoBehaviour
{
    [System.Serializable]
    public class Cell
    {
        public GameObject root;           // сам контейнер ячейки (WordCounterCell_X)
        [HideInInspector] public TMP_Text tmp; // кэш TMP внутри
    }

    [Header("Список ячеек слева направо (по порядку отображения)")]
    [SerializeField] private List<Cell> cells = new List<Cell>();

    [Tooltip("Если true - текст выравнивается по правому краю (полезно для чисел).")]
    [SerializeField] private bool rightAlign = true;

    void Awake()
    {
        // Кэшируем ссылки на TMP в каждой ячейке
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].root != null)
            {
                cells[i].tmp = cells[i].root.GetComponentInChildren<TMP_Text>(true);
                if (cells[i].tmp == null)
                    Debug.LogWarning($"[CellsTextMapper] В ячейке {cells[i].root.name} не найден TMP_Text.");
            }
            else
            {
                Debug.LogWarning($"[CellsTextMapper] Пустая ссылка на root ячейки по индексу {i}.");
            }
        }
    }

    void Start()
    {
        UpdateWordsAmount();
    }
    /// <summary>
    /// Заполняет ячейки символами строки. Лишние ячейки скрывает.
    /// </summary>

    public void UpdateWordsAmount()
    {
        int WordsTotal = GameDataManager.GetTotalSolvedWords();
        SetText(WordsTotal.ToString());
    }

    public void SetText(string input)
    {
        if (input == null) input = "";

        int visibleCount = Mathf.Min(input.Length, cells.Count);

        // Сначала выключаем все
        for (int i = 0; i < cells.Count; i++)
            if (cells[i].root) cells[i].root.SetActive(false);

        // Ничего показывать - выходим
        if (visibleCount == 0) return;

        // Стартовый индекс в ячейках в зависимости от выравнивания
        int startCell = rightAlign ? cells.Count - visibleCount : 0;

        for (int i = 0; i < visibleCount; i++)
        {
            int cellIndex = startCell + i;
            var cell = cells[cellIndex];

            if (cell.root == null || cell.tmp == null) continue;

            cell.root.SetActive(true);
            cell.tmp.text = input[i].ToString();
        }
    }

    /// <summary>
    /// Быстрый сбор ячеек из прямых детей текущего объекта.
    /// Порядок - как в иерархии (слева направо).
    /// </summary>
    [ContextMenu("Auto Collect Cells (children)")]
    private void AutoCollect()
    {
        cells.Clear();
        foreach (Transform child in transform)
        {
            var c = new Cell { root = child.gameObject };
            c.tmp = child.GetComponentInChildren<TMP_Text>(true);
            cells.Add(c);
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        Debug.Log($"[CellsTextMapper] Собрано ячеек: {cells.Count}");
    }
}
