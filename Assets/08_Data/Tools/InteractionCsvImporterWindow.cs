#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InteractionCsvImporterWindow : EditorWindow
{
    [Header("CSV TextAssets")]
    public TextAsset interactionCsv;      // 상호작용 CSV
    public TextAsset interactionListCsv;  // 리스트 CSV

    [Header("Target ScriptableObjects")]
    public InteractionTableSO interactionTable;
    public InteractionListTableSO interactionListTable;

    [MenuItem("GameData/Interaction/Import CSV")]
    public static void OpenWindow()
    {
        GetWindow<InteractionCsvImporterWindow>("Interaction CSV Importer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("CSV Source", EditorStyles.boldLabel);
        interactionCsv = (TextAsset)EditorGUILayout.ObjectField("Interaction CSV", interactionCsv, typeof(TextAsset), false);
        interactionListCsv = (TextAsset)EditorGUILayout.ObjectField("Interaction List CSV", interactionListCsv, typeof(TextAsset), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Target ScriptableObjects", EditorStyles.boldLabel);
        interactionTable = (InteractionTableSO)EditorGUILayout.ObjectField("InteractionTableSO", interactionTable, typeof(InteractionTableSO), false);
        interactionListTable = (InteractionListTableSO)EditorGUILayout.ObjectField("InteractionListTableSO", interactionListTable, typeof(InteractionListTableSO), false);

        EditorGUILayout.Space();

        GUI.enabled = interactionCsv != null && interactionTable != null;
        if (GUILayout.Button("Import Interaction CSV"))
        {
            ImportInteractionCsv();
        }

        GUI.enabled = interactionListCsv != null && interactionListTable != null;
        if (GUILayout.Button("Import Interaction List CSV"))
        {
            ImportInteractionListCsv();
        }

        GUI.enabled = true;
    }

    // ==========================
    //  상호작용 CSV 임포트
    // ==========================
    private void ImportInteractionCsv()
    {
        if (interactionCsv == null || interactionTable == null)
        {
            Debug.LogError("[InteractionCsvImporter] Interaction CSV 또는 Table SO가 비어 있습니다.");
            return;
        }

        Undo.RecordObject(interactionTable, "Import Interaction CSV");
        interactionTable.rows.Clear();

        string[] lines = interactionCsv.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        bool isHeader = true;
        foreach (var rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;
            if (line.StartsWith("#"))
                continue;

            if (isHeader)
            {
                // 첫 줄은 헤더
                isHeader = false;
                continue;
            }

            string[] cols = SplitCsvLine(line);
            if (cols.Length < 8)
            {
                Debug.LogWarning($"[InteractionCsvImporter] 컬럼 수 부족: {line}");
                continue;
            }

            var row = new InteractionRow
            {
                interactionId = cols[0].Trim(),
                interactionText = cols[1].Trim(),
                category = ParseCategory(cols[2]),
                reqType = ParseReqType(cols[3]),
                reqValue = cols[4].Trim(),
                resultType = ParseResultType(cols[5]),
                target = cols[6].Trim(),
                resultValueRaw = cols[7].Trim()
            };

            interactionTable.rows.Add(row);
        }

        EditorUtility.SetDirty(interactionTable);
        AssetDatabase.SaveAssets();
        Debug.Log($"[InteractionCsvImporter] Interaction CSV 임포트 완료. rows={interactionTable.rows.Count}");
    }

    // ==========================
    //  리스트 CSV 임포트
    // ==========================
    private void ImportInteractionListCsv()
    {
        if (interactionListCsv == null || interactionListTable == null)
        {
            Debug.LogError("[InteractionCsvImporter] InteractionList CSV 또는 Table SO가 비어 있습니다.");
            return;
        }

        Undo.RecordObject(interactionListTable, "Import InteractionList CSV");
        interactionListTable.rows.Clear();

        string[] lines = interactionListCsv.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        bool isHeader = true;
        foreach (var rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;
            if (line.StartsWith("#"))
                continue;

            if (isHeader)
            {
                isHeader = false;
                continue;
            }

            string[] cols = SplitCsvLine(line);
            if (cols.Length < 6)
            {
                Debug.LogWarning($"[InteractionCsvImporter] List 컬럼 수 부족: {line}");
                continue;
            }

            var listRow = new InteractionListRow
            {
                listId = cols[0].Trim(),
                targetId = cols[1].Trim(),
                priority = ParseInt(cols[2], 0),
                conditionKey = cols[3].Trim(),
                conditionValue = cols[4].Trim(),
            };

            // LinkedInteraction: "I_N001_01, I_N001_02, I_N001_03"
            string linked = cols[5].Trim();
            linked = TrimQuotes(linked);

            if (!string.IsNullOrEmpty(linked))
            {
                var parts = linked.Split(',');
                foreach (var p in parts)
                {
                    var id = p.Trim();
                    if (!string.IsNullOrEmpty(id))
                        listRow.linkedInteractionIds.Add(id);
                }
            }

            interactionListTable.rows.Add(listRow);
        }

        EditorUtility.SetDirty(interactionListTable);
        AssetDatabase.SaveAssets();
        Debug.Log($"[InteractionCsvImporter] InteractionList CSV 임포트 완료. rows={interactionListTable.rows.Count}");
    }

    // ==========================
    //  CSV 헬퍼들
    // ==========================

    private static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // 따옴표 토글
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString().Trim());
                current.Length = 0;
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString().Trim());
        }

        // 각 필드 양끝의 따옴표 제거
        for (int i = 0; i < result.Count; i++)
        {
            result[i] = TrimQuotes(result[i]);
        }

        return result.ToArray();
    }

    private static string TrimQuotes(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        s = s.Trim();
        if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
        {
            s = s.Substring(1, s.Length - 2);
        }
        return s;
    }

    private static int ParseInt(string s, int defaultValue)
    {
        s = s.Trim();
        if (int.TryParse(s, out int v))
            return v;
        return defaultValue;
    }

    private static InteractionCategory ParseCategory(string s)
    {
        s = s.Trim();
        return s switch
        {
            "대화" => InteractionCategory.Talk,
            "설득" => InteractionCategory.Persuade,
            "수리" => InteractionCategory.Fix,
            _ => InteractionCategory.Unknown
        };
    }

    private static InteractionReqType ParseReqType(string s)
    {
        s = s.Trim();
        if (string.Equals(s, "None", StringComparison.OrdinalIgnoreCase))
            return InteractionReqType.None;
        if (string.Equals(s, "Item", StringComparison.OrdinalIgnoreCase))
            return InteractionReqType.Item;
        if (string.Equals(s, "State", StringComparison.OrdinalIgnoreCase))
            return InteractionReqType.State;
        if (string.Equals(s, "Stat", StringComparison.OrdinalIgnoreCase))
            return InteractionReqType.Stat;

        return InteractionReqType.None;
    }

    private static InteractionResultType ParseResultType(string s)
    {
        s = s.Trim();
        if (string.Equals(s, "ChangeType", StringComparison.OrdinalIgnoreCase))
            return InteractionResultType.ChangeType;
        if (string.Equals(s, "ShowText", StringComparison.OrdinalIgnoreCase))
            return InteractionResultType.ShowText;
        if (string.Equals(s, "ModStat", StringComparison.OrdinalIgnoreCase))
            return InteractionResultType.ModStat;
        if (string.Equals(s, "VolumeChange", StringComparison.OrdinalIgnoreCase))
            return InteractionResultType.VolumeChange;
        if (string.Equals(s, "GameEnd", StringComparison.OrdinalIgnoreCase))
            return InteractionResultType.GameEnd;
        if (string.Equals(s, "PlayCutScene", StringComparison.OrdinalIgnoreCase))
            return InteractionResultType.PlayCutScene;

        return InteractionResultType.None;
    }
}
#endif
