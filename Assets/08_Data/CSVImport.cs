#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using SleepGame.Data;

public class GameDataImportWindow : EditorWindow
{
    private MasterGameDataSO masterData;

    [Header("CSV Files (TextAsset)")]
    private TextAsset neighborsCsv;
    private TextAsset distractionsCsv;

    private bool strictValidation = true;
    private bool clearBeforeImport = true;

    private string lastResultMessage = "";

    [MenuItem("GameData/CSV Import")]
    public static void Open()
    {
        GetWindow<GameDataImportWindow>("Game Data Import");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Master Game Data", EditorStyles.boldLabel);
        masterData = (MasterGameDataSO)EditorGUILayout.ObjectField(
            "MasterGameDataSO",
            masterData,
            typeof(MasterGameDataSO),
            false
        );

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("CSV Files", EditorStyles.boldLabel);
        neighborsCsv = (TextAsset)EditorGUILayout.ObjectField(
            "Neighbors CSV",
            neighborsCsv,
            typeof(TextAsset),
            false
        );
        distractionsCsv = (TextAsset)EditorGUILayout.ObjectField(
            "Distractions CSV",
            distractionsCsv,
            typeof(TextAsset),
            false
        );

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        strictValidation = EditorGUILayout.Toggle("Strict Validation", strictValidation);
        clearBeforeImport = EditorGUILayout.Toggle("Clear Before Import", clearBeforeImport);

        EditorGUILayout.Space();
        if (GUILayout.Button("Import All", GUILayout.Height(30)))
        {
            ImportAll();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Last Result", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(lastResultMessage, MessageType.Info);
    }

    private void ImportAll()
    {
        if (masterData == null)
        {
            lastResultMessage = "MasterGameDataSO is not assigned.";
            Debug.LogError(lastResultMessage);
            return;
        }

        if (neighborsCsv == null || distractionsCsv == null)
        {
            lastResultMessage = "CSV TextAssets are not assigned.";
            Debug.LogError(lastResultMessage);
            return;
        }

        // Neighbor / Distraction Table SO 확보
        var neighborTable = masterData.neighborTable;
        var distractionTable = masterData.distractionTable;

        if (neighborTable == null || distractionTable == null)
        {
            lastResultMessage = "NeighborTableSO or DistractionTableSO is not assigned in MasterGameDataSO.";
            Debug.LogError(lastResultMessage);
            return;
        }

        try
        {
            int importedNeighbors = ImportNeighbors(neighborsCsv, neighborTable);
            int importedDistractions = ImportDistractions(distractionsCsv, distractionTable, neighborTable);

            EditorUtility.SetDirty(neighborTable);
            EditorUtility.SetDirty(distractionTable);
            EditorUtility.SetDirty(masterData);
            AssetDatabase.SaveAssets();

            lastResultMessage = $"Import Success\n" +
                                $"Neighbors: {importedNeighbors}\n" +
                                $"Distractions: {importedDistractions}";
            Debug.Log(lastResultMessage);
        }
        catch (Exception ex)
        {
            lastResultMessage = "Import failed: " + ex.Message;
            Debug.LogError(ex);
        }
    }

    private int ImportNeighbors(TextAsset csv, NeighborTableSO table)
    {
        if (clearBeforeImport)
            table.neighbors.Clear();

        var lines = SplitLines(csv.text);
        if (lines.Count <= 1)
            throw new Exception("Neighbors CSV has no data rows.");

        // 첫 줄은 헤더
        var header = SplitCsvLine(lines[0]);
        int idxNeighborId = Array.IndexOf(header, "NeighborID");
        int idxName = Array.IndexOf(header, "Name");
        int idxLayoutId = Array.IndexOf(header, "LayoutID");
        int idxDesc = Array.IndexOf(header, "Description");

        if (idxNeighborId < 0 || idxName < 0 || idxLayoutId < 0)
            throw new Exception("Neighbors CSV header must contain NeighborID, Name, LayoutID.");

        var idSet = new HashSet<string>();
        int imported = 0;

        for (int i = 1; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var cols = SplitCsvLine(line);
            if (cols.Length <= idxNeighborId)
                continue;

            string id = cols[idxNeighborId].Trim();
            if (string.IsNullOrEmpty(id))
            {
                LogOrThrow($"[Neighbors] Row {i + 1}: NeighborID is empty.");
                continue;
            }

            if (idSet.Contains(id))
            {
                LogOrThrow($"[Neighbors] Duplicate NeighborID: {id} at row {i + 1}.");
                continue;
            }

            idSet.Add(id);

            var row = new NeighborDataRow
            {
                neighborId = id,
                displayName = SafeGet(cols, idxName),
                layoutId = SafeGet(cols, idxLayoutId),
                description = idxDesc >= 0 ? SafeGet(cols, idxDesc) : ""
            };

            table.neighbors.Add(row);
            imported++;
        }

        return imported;
    }

    private int ImportDistractions(TextAsset csv, DistractionTableSO table, NeighborTableSO neighborTable)
    {
        if (clearBeforeImport)
            table.distractions.Clear();

        var lines = SplitLines(csv.text);
        if (lines.Count <= 1)
            throw new Exception("Distractions CSV has no data rows.");

        var header = SplitCsvLine(lines[0]);
        int idxDistractionId = Array.IndexOf(header, "DistractionID");
        int idxOwnerId = Array.IndexOf(header, "OwnerID");
        int idxSourceId = Array.IndexOf(header, "SourceID");
        int idxTag = Array.IndexOf(header, "Tag");
        int idxIntensity = Array.IndexOf(header, "Intensity");
        int idxSfxId = Array.IndexOf(header, "SfxID");
        int idxPlaceId = Array.IndexOf(header, "PlaceID");
        int idxDesc = Array.IndexOf(header, "Description");

        if (idxDistractionId < 0 || idxOwnerId < 0)
            throw new Exception("Distractions CSV header must contain DistractionID, OwnerID.");

        var neighborIdSet = new HashSet<string>(neighborTable.neighbors.Select(n => n.neighborId));
        var idSet = new HashSet<string>();
        int imported = 0;

        for (int i = 1; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var cols = SplitCsvLine(line);
            if (cols.Length <= idxDistractionId)
                continue;

            string id = cols[idxDistractionId].Trim();
            if (string.IsNullOrEmpty(id))
            {
                LogOrThrow($"[Distractions] Row {i + 1}: DistractionID is empty.");
                continue;
            }

            if (idSet.Contains(id))
            {
                LogOrThrow($"[Distractions] Duplicate DistractionID: {id} at row {i + 1}.");
                continue;
            }

            idSet.Add(id);

            string ownerId = SafeGet(cols, idxOwnerId);
            if (string.IsNullOrEmpty(ownerId))
            {
                LogOrThrow($"[Distractions] Row {i + 1}: OwnerID is empty. (DistractionID={id})");
                continue;
            }

            if (!neighborIdSet.Contains(ownerId))
            {
                LogOrThrow($"[Distractions] Row {i + 1}: OwnerID '{ownerId}' not found in NeighborTable. (DistractionID={id})");
                if (strictValidation)
                    continue;
            }

            int intensity = 0;
            if (idxIntensity >= 0)
                int.TryParse(SafeGet(cols, idxIntensity), out intensity);

            var row = new DistractionDataRow
            {
                distractionId = id,
                ownerId = ownerId,
                sourceId = idxSourceId >= 0 ? SafeGet(cols, idxSourceId) : "",
                tag = idxTag >= 0 ? SafeGet(cols, idxTag) : "",
                intensity = intensity,
                sfxId = idxSfxId >= 0 ? SafeGet(cols, idxSfxId) : "",
                placeId = idxPlaceId >= 0 ? SafeGet(cols, idxPlaceId) : "",
                description = idxDesc >= 0 ? SafeGet(cols, idxDesc) : ""
            };

            table.distractions.Add(row);
            imported++;
        }

        return imported;
    }

    // =========================
    // CSV 유틸 (심플 버전)
    // =========================

    private List<string> SplitLines(string text)
    {
        return text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Split('\n')
            .ToList();
    }

    // 프로토타입용 심플 파서 (따옴표/콤마 포함 케이스는 미지원)
    private string[] SplitCsvLine(string line)
    {
        return line.Split(',');
    }

    private string SafeGet(string[] cols, int index)
    {
        if (index < 0 || index >= cols.Length)
            return "";
        return cols[index].Trim();
    }

    private void LogOrThrow(string message)
    {
        if (strictValidation)
            throw new Exception(message);

        Debug.LogWarning(message);
    }
}
#endif
