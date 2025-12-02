#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Text;

public class GameDataImportWindow : EditorWindow
{
    private MasterGameDataSO masterData;

    [Header("CSV Files (TextAsset)")]
    private TextAsset neighborsCsv;
    private TextAsset distractionsCsv;
    private TextAsset placesCsv;   // ★ 추가

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
        placesCsv = (TextAsset)EditorGUILayout.ObjectField(          // ★ 추가
            "Places CSV",
            placesCsv,
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

        if (neighborsCsv == null || distractionsCsv == null || placesCsv == null)
        {
            lastResultMessage = "CSV TextAssets are not assigned (Neighbors/Distractions/Places).";
            Debug.LogError(lastResultMessage);
            return;
        }

        var neighborTable = masterData.neighborTable;
        var distractionTable = masterData.distractionTable;
        var placeTable = masterData.placeTable;   // ★ 추가

        if (neighborTable == null || distractionTable == null || placeTable == null)
        {
            lastResultMessage = "NeighborTableSO or DistractionTableSO or PlaceTableSO is not assigned in MasterGameDataSO.";
            Debug.LogError(lastResultMessage);
            return;
        }

        try
        {
            int importedNeighbors = ImportNeighbors(neighborsCsv, neighborTable);
            int importedDistractions = ImportDistractions(distractionsCsv, distractionTable, neighborTable);
            int importedPlaces = ImportPlaces(placesCsv, placeTable);   // ★ 추가

            EditorUtility.SetDirty(neighborTable);
            EditorUtility.SetDirty(distractionTable);
            EditorUtility.SetDirty(placeTable);   // ★ 추가
            EditorUtility.SetDirty(masterData);
            AssetDatabase.SaveAssets();

            lastResultMessage = $"Import Success\n" +
                                $"Neighbors: {importedNeighbors}\n" +
                                $"Distractions: {importedDistractions}\n" +
                                $"Places: {importedPlaces}";
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
        if (csv == null)
            throw new Exception("[Neighbors] CSV TextAsset is null.");

        if (table == null)
            throw new Exception("[Neighbors] NeighborTableSO is null.");

        if (table.neighbors == null)
            table.neighbors = new List<NeighborDataRow>();
        else if (clearBeforeImport)
            table.neighbors.Clear();

        var lines = SplitLines(csv.text);
        if (lines.Count == 0)
            throw new Exception("Neighbors CSV has no lines.");

        int lineIndex = 0;

        // 1) 헤더 라인 찾기 (완전 빈 줄 스킵)
        string headerLine = null;
        for (; lineIndex < lines.Count; lineIndex++)
        {
            if (!string.IsNullOrWhiteSpace(lines[lineIndex]))
            {
                headerLine = lines[lineIndex];
                break;
            }
        }

        if (headerLine == null)
            throw new Exception("Neighbors CSV has no header row.");

        // 2) 헤더 파싱 + 열 이름 트림
        var headerCells = SplitCsvLineToList(headerLine);
        var headerIndex = new Dictionary<string, int>();

        for (int i = 0; i < headerCells.Count; i++)
        {
            string raw = headerCells[i] ?? string.Empty;
            string trimmed = raw.Trim().Trim('\uFEFF'); // BOM + 공백 제거

            if (string.IsNullOrEmpty(trimmed))
                continue;

            if (!headerIndex.ContainsKey(trimmed))
                headerIndex.Add(trimmed, i);
        }

        int GetHeader(string name, bool required = true, params string[] aliases)
        {
            if (headerIndex.TryGetValue(name, out var idx))
                return idx;

            foreach (var alias in aliases)
            {
                if (headerIndex.TryGetValue(alias, out idx))
                    return idx;
            }

            if (required)
                throw new Exception($"[Neighbors] Header '{name}' not found.");

            return -1;
        }

        // 필수 컬럼들
        int idxNeighborId = GetHeader("NeighborID");
        int idxName = GetHeader("Name");
        int idxLayoutId = GetHeader("LayoutID");
        // 설명은 Info 또는 Description 둘 다 허용
        int idxDesc = GetHeader("Description", required: false, aliases: new[] { "Info" });

        var idSet = new HashSet<string>();
        int imported = 0;
        int rowNumber = 0; // 데이터 기준 row 카운트

        // 3) 데이터 라인 파싱
        for (lineIndex = lineIndex + 1; lineIndex < lines.Count; lineIndex++)
        {
            string line = lines[lineIndex];

            // 완전 빈 줄이면 스킵
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var cells = SplitCsvLineToList(line);

            string neighborId = GetCellSafe(cells, idxNeighborId).Trim();
            string name = GetCellSafe(cells, idxName).Trim();
            string layoutId = GetCellSafe(cells, idxLayoutId).Trim();
            string desc = idxDesc >= 0 ? GetCellSafe(cells, idxDesc).Trim() : string.Empty;

            // 빈 줄 Skip: 모든 주요 필드가 비어 있으면 무시
            bool allEmpty =
                string.IsNullOrWhiteSpace(neighborId) &&
                string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(layoutId) &&
                string.IsNullOrWhiteSpace(desc);

            if (allEmpty)
                continue;

            rowNumber++;

            // 필수값 체크
            if (string.IsNullOrWhiteSpace(neighborId))
            {
                LogOrThrow($"[Neighbors] Row {rowNumber}: NeighborID is empty. (CSV line {lineIndex + 1})");
                continue;
            }

            if (idSet.Contains(neighborId))
            {
                LogOrThrow($"[Neighbors] Duplicate NeighborID '{neighborId}' at CSV line {lineIndex + 1}.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(layoutId))
            {
                LogOrThrow($"[Neighbors] Row {rowNumber}: LayoutID is empty. (NeighborID={neighborId})");
                continue;
            }

            idSet.Add(neighborId);

            var row = new NeighborDataRow
            {
                neighborId = neighborId,
                displayName = name,
                layoutId = layoutId,
                description = desc
            };

            table.neighbors.Add(row);
            imported++;
        }

        return imported;
    }


    private int ImportDistractions(TextAsset csv, DistractionTableSO table, NeighborTableSO neighborTable)
    {
        if (csv == null)
            throw new Exception("[Distractions] CSV TextAsset is null.");

        if (table == null)
            throw new Exception("[Distractions] DistractionTableSO is null.");

        if (neighborTable == null)
            throw new Exception("[Distractions] NeighborTableSO is null.");

        if (table.distractions == null)
            table.distractions = new List<DistractionDataRow>();
        else if (clearBeforeImport)
            table.distractions.Clear();

        var lines = SplitLines(csv.text);
        if (lines.Count == 0)
            throw new Exception("[Distractions] CSV is empty.");

        int lineIndex = 0;

        // 1) 헤더 라인 찾기
        string headerLine = null;
        for (; lineIndex < lines.Count; lineIndex++)
        {
            if (!string.IsNullOrWhiteSpace(lines[lineIndex]))
            {
                headerLine = lines[lineIndex];
                break;
            }
        }

        if (headerLine == null)
            throw new Exception("[Distractions] No header found in CSV.");

        // 2) 헤더 파싱
        var headerCells = SplitCsvLineToList(headerLine);
        var headerIndex = new Dictionary<string, int>();

        for (int i = 0; i < headerCells.Count; i++)
        {
            string raw = headerCells[i] ?? string.Empty;
            string trimmed = raw.Trim().Trim('\uFEFF'); // BOM 제거

            if (string.IsNullOrEmpty(trimmed))
                continue;

            if (!headerIndex.ContainsKey(trimmed))
                headerIndex.Add(trimmed, i);
        }

        int GetHeader(string name, bool required = true, params string[] aliases)
        {
            if (headerIndex.TryGetValue(name, out var idx))
                return idx;

            foreach (var alias in aliases)
            {
                if (headerIndex.TryGetValue(alias, out idx))
                    return idx;
            }

            if (required)
                throw new Exception($"[Distractions] Header '{name}' not found.");

            return -1;
        }

        // 필수 컬럼
        int idxDistractionId = GetHeader("DistractionID");
        int idxOwnerId = GetHeader("OwnerID");

        // 선택 컬럼
        int idxSourceId = GetHeader("SourceID", required: false);
        int idxTag = GetHeader("Tag", required: false);
        int idxIntensity = GetHeader("Intensity", required: false);
        int idxSfxId = GetHeader("SfxID", required: false, aliases: new[] { "SFXID", "Sfx" });
        int idxPlaceId = GetHeader("PlaceID", required: false);
        int idxDesc = GetHeader("Description", required: false, aliases: new[] { "Info" });

        var knownNeighborIds = new HashSet<string>(neighborTable.neighbors.Select(n => n.neighborId));
        var idSet = new HashSet<string>();

        int imported = 0;
        int rowNumber = 0;

        // 3) 데이터 파싱
        for (lineIndex = lineIndex + 1; lineIndex < lines.Count; lineIndex++)
        {
            string line = lines[lineIndex];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var cells = SplitCsvLineToList(line);

            string disId = GetCellSafe(cells, idxDistractionId).Trim();
            string ownerId = GetCellSafe(cells, idxOwnerId).Trim();

            // 빈 줄 Skip (모든 주요 컬럼 비면 무시)
            bool allEmpty =
                string.IsNullOrWhiteSpace(disId) &&
                string.IsNullOrWhiteSpace(ownerId) &&
                string.IsNullOrWhiteSpace(GetCellSafe(cells, idxTag)) &&
                string.IsNullOrWhiteSpace(GetCellSafe(cells, idxPlaceId));

            if (allEmpty)
                continue;

            rowNumber++;

            // 필수 컬럼 체크
            if (string.IsNullOrWhiteSpace(disId))
            {
                LogOrThrow($"[Distractions] Row {rowNumber}: DistractionID is empty. (CSV line {lineIndex + 1})");
                continue;
            }

            if (idSet.Contains(disId))
            {
                LogOrThrow($"[Distractions] Duplicate ID '{disId}' at CSV line {lineIndex + 1}.");
                continue;
            }

            idSet.Add(disId);

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                LogOrThrow($"[Distractions] Row {rowNumber}: OwnerID is empty. (DistractionID={disId})");
                continue;
            }

            if (!knownNeighborIds.Contains(ownerId))
            {
                LogOrThrow($"[Distractions] OwnerID '{ownerId}' is not found in NeighborTable. (DistractionID={disId})");

                if (strictValidation)
                    continue;
            }

            // intensity parsing
            int intensity = 0;
            if (idxIntensity >= 0)
                int.TryParse(GetCellSafe(cells, idxIntensity), out intensity);

            var row = new DistractionDataRow
            {
                distractionId = disId,
                ownerId = ownerId,
                sourceId = GetCellSafe(cells, idxSourceId),
                tag = GetCellSafe(cells, idxTag),
                intensity = intensity,
                sfxId = GetCellSafe(cells, idxSfxId),
                placeId = GetCellSafe(cells, idxPlaceId),
                description = GetCellSafe(cells, idxDesc)
            };

            table.distractions.Add(row);
            imported++;
        }

        return imported;
    }

    private int ImportPlaces(TextAsset csv, PlaceTableSO table)
    {
        if (clearBeforeImport)
            table.places.Clear();

        var lines = SplitLines(csv.text);
        if (lines.Count <= 1)
            throw new Exception("Places CSV has no data rows.");

        // ----- 헤더 처리 (Trim 포함) -----
        var rawHeader = SplitCsvLine(lines[0]);
        var header = rawHeader
            .Select(h => h.Trim())
            .ToArray();

        int idxPlaceId = Array.IndexOf(header, "PlaceID");
        int idxFloor = Array.IndexOf(header, "Floor");
        int idxDistanceLevel = Array.IndexOf(header, "DistanceLevel");

        if (idxPlaceId < 0)
            throw new Exception("Places CSV header must contain PlaceID.");

        var idSet = new HashSet<string>();
        int imported = 0;

        for (int i = 1; i < lines.Count; i++)
        {
            var line = lines[i];

            // 줄 자체가 공백이면 스킵
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var cols = SplitCsvLine(line);

            // " , , , " 처럼 콤마만 있고 전부 공백인 행도 스킵
            bool allEmpty = true;
            for (int c = 0; c < cols.Length; c++)
            {
                if (!string.IsNullOrWhiteSpace(cols[c]))
                {
                    allEmpty = false;
                    break;
                }
            }
            if (allEmpty)
                continue;

            string placeId = SafeGet(cols, idxPlaceId);
            if (string.IsNullOrEmpty(placeId))
            {
                LogOrThrow($"[Places] Row {i + 1}: PlaceID is empty.");
                continue;
            }

            if (idSet.Contains(placeId))
            {
                LogOrThrow($"[Places] Duplicate PlaceID: {placeId} at row {i + 1}.");
                continue;
            }
            idSet.Add(placeId);

            int floor = 0;
            if (idxFloor >= 0)
                int.TryParse(SafeGet(cols, idxFloor), out floor);

            int distanceLevel = 0;
            if (idxDistanceLevel >= 0)
                int.TryParse(SafeGet(cols, idxDistanceLevel), out distanceLevel);

            var row = new PlaceDataRow
            {
                placeId = placeId,
                floor = floor,
                distanceLevel = distanceLevel
            };

            table.places.Add(row);
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

    /// <summary>
    /// 따옴표/콤마를 고려해서 한 줄을 파싱 (리스트 버전)
    /// 예: "계속 역기를 내려놓으며, 쿵쿵거린다." 같은 필드 지원
    /// </summary>
    private List<string> SplitCsvLineToList(string line)
    {
        var result = new List<string>();

        if (string.IsNullOrEmpty(line))
        {
            result.Add(string.Empty);
            return result;
        }

        bool inQuotes = false;
        var sb = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '\"')
            {
                // "" → " 로 처리 (CSV 이스케이프)
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    sb.Append('\"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Length = 0;
            }
            else
            {
                sb.Append(c);
            }
        }

        // 마지막 필드
        result.Add(sb.ToString());

        return result;
    }

    /// <summary>
    /// List 기반 CSV 셀 안전 접근 (범위 밖이면 빈 문자열)
    /// </summary>
    private string GetCellSafe(List<string> cells, int index)
    {
        if (cells == null || index < 0 || index >= cells.Count)
            return string.Empty;

        return cells[index] ?? string.Empty;
    }
}
#endif
