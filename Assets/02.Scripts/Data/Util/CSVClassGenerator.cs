using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CSV 파일을 분석해서 Data Class + ScriptableObject 스크립트를 자동 생성하고,
/// 컴파일 완료 후 실제 .asset 파일까지 생성 + 데이터를 채우는 에디터 툴.
///
/// 사용 흐름:
///   1. [▶ Generate Scripts] → Data Class .cs + SO .cs 생성 → 컴파일 대기
///   2. 컴파일 완료 후 [▶ Import CSV Data] 버튼 활성화 → .asset 생성 + dataList 채우기
/// </summary>
[InitializeOnLoad]
public class CsvAutoGenerator : EditorWindow
{
    // ───────────────────────────── 상수 ─────────────────────────────
    private const int COMMENT_ROW = 0;
    private const int HEADER_ROW  = 1;
    private const int TYPE_ROW    = 2;
    private const int DATA_START  = 3;

    // EditorPrefs 키 (컴파일 경계를 넘어 상태 유지)
    private const string PREF_PENDING       = "CsvAutoGen_PendingImport";
    private const string PREF_CSV_FOLDER    = "CsvAutoGen_CsvFolder";
    private const string PREF_CLASS_OUTPUT  = "CsvAutoGen_ClassOutput";
    private const string PREF_SO_OUTPUT     = "CsvAutoGen_SoOutput";
    private const string PREF_OVERWRITE     = "CsvAutoGen_Overwrite";
    private const string PREF_EXCLUDED      = "CsvAutoGen_Excluded";   // 제외 파일명 목록 ("|" 구분)

    // ───────────────────────────── 상태 ─────────────────────────────
    private string _csvFolderPath    = "Assets/00.Resources/Resources/Data/CSV";
    private string _classOutputPath  = "Assets/02.Scripts/Data";
    private string _soOutputPath     = "Assets/00.Resources/Resources/Data/ScriptableObjects";
    private bool _overwriteExisting = true;

    private bool _pendingImport = false;   // 컴파일 완료 대기 중 여부

    // 제외 목록: key=파일명(확장자 제외), value=true이면 제외
    private Dictionary<string, bool> _excludeMap = new Dictionary<string, bool>();
    private bool _showExcludeList = false;
    private Vector2 _excludeScroll;

    private Vector2 _scroll;
    private List<string> _log = new List<string>();

    // ───────────────────────────── InitializeOnLoad ──────────────────
    /// <summary>
    /// 컴파일이 완료될 때마다 호출됨.
    /// PREF_PENDING 플래그가 있으면 창을 열고 Import 단계로 복원.
    /// </summary>
    static CsvAutoGenerator()
    {
        EditorApplication.delayCall += OnAfterCompile;
    }

    static void OnAfterCompile()
    {
        if (!EditorPrefs.GetBool(PREF_PENDING, false)) return;

        // 창이 이미 열려 있으면 재사용, 없으면 새로 열기
        var win = GetWindow<CsvAutoGenerator>("CSV Auto Generator");
        win._pendingImport   = true;
        win._csvFolderPath   = EditorPrefs.GetString(PREF_CSV_FOLDER,   win._csvFolderPath);
        win._classOutputPath = EditorPrefs.GetString(PREF_CLASS_OUTPUT, win._classOutputPath);
        win._soOutputPath    = EditorPrefs.GetString(PREF_SO_OUTPUT,    win._soOutputPath);
        win._overwriteExisting = EditorPrefs.GetBool(PREF_OVERWRITE, true);
        win.LoadExcludeMap();
        win.Log("[OK] 컴파일 완료! [▶ Import CSV Data] 버튼으로 .asset 파일을 생성하세요.");
        win.Repaint();
    }

    // ───────────────────────────── 메뉴 ─────────────────────────────
    [MenuItem("Tools/CSV Auto Generator")]
    public static void Open() => GetWindow<CsvAutoGenerator>("CSV Auto Generator");

    private void OnEnable() => LoadExcludeMap();

    // ───────────────────────────── GUI ──────────────────────────────
    private void OnGUI()
    {
        GUILayout.Label("⚙ CSV 자동 생성기", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        DrawPathField("CSV 폴더 경로",   ref _csvFolderPath,   true);
        DrawPathField("Class 출력 경로", ref _classOutputPath, true);
        DrawPathField("SO 출력 경로",    ref _soOutputPath,    true);

        EditorGUILayout.Space(6);
        _overwriteExisting = EditorGUILayout.Toggle("기존 파일 덮어쓰기", _overwriteExisting);

        EditorGUILayout.Space(10);

        // ── Step 1: 스크립트 생성 버튼
        GUI.backgroundColor = _pendingImport
            ? new Color(0.5f, 0.5f, 0.5f)   // 대기 중엔 회색
            : new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("▶  1. Generate Scripts  (Data Class + SO 스크립트)", GUILayout.Height(36)))
        {
            if (!_pendingImport)
                RunGenerateScripts();
        }

        EditorGUILayout.Space(4);

        // ── Step 2: CSV 데이터 임포트 버튼 (컴파일 완료 후 활성화)
        GUI.backgroundColor = _pendingImport
            ? new Color(0.3f, 0.6f, 1.0f)   // 활성화 시 파란색
            : new Color(0.5f, 0.5f, 0.5f);
        GUI.enabled = _pendingImport;
        if (GUILayout.Button("▶  2. Import CSV Data  (.asset 생성 + 데이터 채우기)", GUILayout.Height(36)))
            RunImportData();
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;

        if (_pendingImport)
        {
            EditorGUILayout.HelpBox("스크립트 컴파일이 완료되었습니다. [Import CSV Data]를 눌러 .asset 파일을 생성하세요.", MessageType.Info);
        }

        // ── 구분선
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(2);

        // ── Reimport: 스크립트 생성 없이 CSV → 기존 SO 데이터만 갱신
        bool canReimport = !_pendingImport;   // 컴파일 대기 중엔 비활성 (타입이 불안정)
        GUI.backgroundColor = canReimport ? new Color(1.0f, 0.75f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
        GUI.enabled = canReimport;
        if (GUILayout.Button("↺  Reimport CSV Data  (스크립트 유지, 데이터만 갱신)", GUILayout.Height(32)))
            RunReimportData();
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        EditorGUILayout.HelpBox("CSV 내용만 바뀌었을 때 사용. 스크립트는 재생성하지 않습니다.", MessageType.None);

        // ── 제외할 CSV 선택 패널
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        EditorGUILayout.BeginHorizontal();
        _showExcludeList = EditorGUILayout.Foldout(_showExcludeList, "제외할 CSV 파일", true);
        if (GUILayout.Button("목록 새로고침", GUILayout.Width(90)))
            RefreshExcludeMap();
        EditorGUILayout.EndHorizontal();

        if (_showExcludeList)
        {
            if (_excludeMap.Count == 0)
            {
                EditorGUILayout.HelpBox("CSV 폴더를 먼저 지정하고 '목록 새로고침'을 누르세요.", MessageType.None);
            }
            else
            {
                _excludeScroll = EditorGUILayout.BeginScrollView(_excludeScroll, GUILayout.Height(120));
                bool changed = false;
                foreach (var key in new List<string>(_excludeMap.Keys))
                {
                    bool prev = _excludeMap[key];
                    bool next = EditorGUILayout.ToggleLeft(key, prev);
                    if (next != prev) { _excludeMap[key] = next; changed = true; }
                }
                EditorGUILayout.EndScrollView();
                if (changed) SaveExcludeMap();

                int excludedCount = 0;
                foreach (var kv in _excludeMap) if (kv.Value) excludedCount++;
                if (excludedCount > 0)
                    EditorGUILayout.HelpBox($"{excludedCount}개 파일이 제외됩니다.", MessageType.Warning);
            }
        }

        // 로그 영역
        EditorGUILayout.Space(8);
        GUILayout.Label("Log", EditorStyles.boldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(200));
        foreach (var line in _log)
        {
            Color prev = GUI.color;
            if (line.StartsWith("[ERR]"))   GUI.color = new Color(1f, 0.4f, 0.4f);
            else if (line.StartsWith("[OK]")) GUI.color = new Color(0.5f, 1f, 0.5f);
            else GUI.color = Color.white;
            GUILayout.Label(line, EditorStyles.wordWrappedLabel);
            GUI.color = prev;
        }
        EditorGUILayout.EndScrollView();

        if (_log.Count > 0 && GUILayout.Button("Clear Log"))
            _log.Clear();
    }

    private void DrawPathField(string label, ref string path, bool isFolder)
    {
        EditorGUILayout.BeginHorizontal();
        path = EditorGUILayout.TextField(label, path);
        if (GUILayout.Button("…", GUILayout.Width(28)))
        {
            string selected = isFolder
                ? EditorUtility.OpenFolderPanel(label, path, "")
                : EditorUtility.OpenFilePanel(label, path, "csv");
            if (!string.IsNullOrEmpty(selected))
            {
                if (selected.StartsWith(Application.dataPath))
                    selected = "Assets" + selected.Substring(Application.dataPath.Length);
                path = selected;
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    // ═══════════════════════════════════════════════════════════════
    //  STEP 1 : 스크립트 생성
    // ═══════════════════════════════════════════════════════════════
    private void RunGenerateScripts()
    {
        _log.Clear();

        string[] csvFiles = GetCsvFiles();
        if (csvFiles == null) return;

        EnsureDirectory(_classOutputPath);

        int ok = 0;
        foreach (string csvPath in csvFiles)
        {
            try   { GenerateScripts(csvPath); ok++; }
            catch (Exception e) { Log($"[ERR] {Path.GetFileName(csvPath)}: {e.Message}"); }
        }

        // 컴파일 완료 후 Import를 재개하기 위한 상태 저장
        EditorPrefs.SetBool  (PREF_PENDING,      true);
        EditorPrefs.SetString(PREF_CSV_FOLDER,   _csvFolderPath);
        EditorPrefs.SetString(PREF_CLASS_OUTPUT, _classOutputPath);
        EditorPrefs.SetString(PREF_SO_OUTPUT,    _soOutputPath);
        EditorPrefs.SetBool  (PREF_OVERWRITE,    _overwriteExisting);

        _pendingImport = true;

        AssetDatabase.Refresh();   // 컴파일 트리거
        Log($"──────────────────────────────");
        Log($"[OK] 스크립트 {ok}/{csvFiles.Length}개 생성 완료. 컴파일 후 Import 버튼을 누르세요.");
    }

    private void GenerateScripts(string csvAbsPath)
    {
        string fileName  = Path.GetFileNameWithoutExtension(csvAbsPath);
        string className = ToPascalCase(fileName);
        string soName    = className + "SO";

        Log($"── {fileName}.csv 스크립트 생성 중...");

        var (headers, typeHints, rows) = ReadCsv(csvAbsPath);
        if (headers == null) return;

        var fields = BuildFields(headers, typeHints, rows);

        // Data Class
        string classCode = BuildDataClass(className, fields, rows);
        WriteFile(_classOutputPath, className + ".cs", classCode);
        Log($"  [OK] {className}.cs");

        // SO 스크립트
        string soCode = BuildScriptableObjectScript(soName, className, fields);
        WriteFile(_classOutputPath, soName + ".cs", soCode);
        Log($"  [OK] {soName}.cs");
    }

    // ═══════════════════════════════════════════════════════════════
    //  STEP 2 : .asset 생성 + 데이터 채우기
    // ═══════════════════════════════════════════════════════════════
    private void RunImportData()
    {
        _log.Clear();
        ExecuteImport();

        // Step 2 완료 → 플래그 해제
        EditorPrefs.SetBool(PREF_PENDING, false);
        _pendingImport = false;
    }

    /// <summary>
    /// Reimport: 스크립트 생성/컴파일 없이 CSV 데이터만 기존 SO에 다시 씀.
    /// SO 타입이 이미 컴파일돼 있어야 하며, .asset이 없으면 새로 생성.
    /// </summary>
    private void RunReimportData()
    {
        _log.Clear();
        Log("↺ Reimport 시작 (스크립트 유지)...");
        ExecuteImport();
    }

    /// <summary>실제 임포트 공통 로직 — RunImportData / RunReimportData 양쪽에서 호출</summary>
    private void ExecuteImport()
    {
        string[] csvFiles = GetCsvFiles();
        if (csvFiles == null) return;

        EnsureDirectory(_soOutputPath);

        int ok = 0;
        foreach (string csvPath in csvFiles)
        {
            try   { ImportDataToAsset(csvPath); ok++; }
            catch (Exception e) { Log($"[ERR] {Path.GetFileName(csvPath)}: {e.Message}"); }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Log($"──────────────────────────────");
        Log($"[OK] 완료! {ok}/{csvFiles.Length}개 .asset 생성/갱신됨");
    }

    private void ImportDataToAsset(string csvAbsPath)
    {
        string fileName  = Path.GetFileNameWithoutExtension(csvAbsPath);
        string className = ToPascalCase(fileName);
        string soName    = className + "SO";
        string assetName = soName + ".asset";

        Log($"── {fileName}.csv → {assetName} 임포트 중...");

        var (headers, typeHints, rows) = ReadCsv(csvAbsPath);
        if (headers == null) return;

        var fields = BuildFields(headers, typeHints, rows);

        // SO 타입 찾기 (컴파일 완료 후이므로 존재해야 함)
        Type soType = FindType(soName);
        if (soType == null)
        {
            Log($"  [ERR] 타입 '{soName}'을 찾을 수 없습니다. 스크립트가 컴파일됐는지 확인하세요.");
            return;
        }
        Type dataType = FindType(className);
        if (dataType == null)
        {
            Log($"  [ERR] 타입 '{className}'을 찾을 수 없습니다.");
            return;
        }

        // 기존 .asset 로드 or 새로 생성
        string assetRelPath = _soOutputPath.TrimEnd('/') + "/" + assetName;
        ScriptableObject so = AssetDatabase.LoadAssetAtPath(assetRelPath, soType) as ScriptableObject;

        bool isNew = (so == null);
        if (isNew)
        {
            so = ScriptableObject.CreateInstance(soType);
        }
        else if (!_overwriteExisting)
        {
            Log($"  [SKIP] {assetName} (이미 존재, 덮어쓰기 OFF)");
            return;
        }

        // dataList 필드 가져오기
        var serializedObj  = new SerializedObject(so);
        var dataListProp   = serializedObj.FindProperty("dataList");
        if (dataListProp == null)
        {
            Log($"  [ERR] '{soName}'에서 dataList 프로퍼티를 찾을 수 없습니다.");
            return;
        }

        dataListProp.ClearArray();

        // DATA_START 행부터 파싱해서 채우기
        for (int r = DATA_START; r < rows.Length; r++)
        {
            string rowTrimmed = rows[r].Trim();
            if (string.IsNullOrEmpty(rowTrimmed)) continue;

            string[] vals = ParseCsvRow(rowTrimmed);

            dataListProp.InsertArrayElementAtIndex(dataListProp.arraySize);
            SerializedProperty elemProp = dataListProp.GetArrayElementAtIndex(dataListProp.arraySize - 1);

            for (int i = 0; i < fields.Count && i < vals.Length; i++)
            {
                var field    = fields[i];
                string rawVal = (i < vals.Length) ? vals[i].Trim() : "";
                SerializedProperty fp = elemProp.FindPropertyRelative(field.Name);
                if (fp == null) continue;

                SetSerializedValue(fp, field, rawVal);
            }
        }

        serializedObj.ApplyModifiedPropertiesWithoutUndo();

        if (isNew)
        {
            AssetDatabase.CreateAsset(so, assetRelPath);
            Log($"  [OK] {assetName} 신규 생성 (dataList: {dataListProp.arraySize}개)");
        }
        else
        {
            EditorUtility.SetDirty(so);
            Log($"  [OK] {assetName} 갱신 (dataList: {dataListProp.arraySize}개)");
        }
    }

    // ───────────────────────────── CSV 파싱 ──────────────────────────
    (string[] headers, string[] typeHints, string[] rows) ReadCsv(string csvAbsPath)
    {
        string raw = File.ReadAllText(csvAbsPath, Encoding.UTF8).Trim();
        // 개행 기준으로 분리 (따옴표 내부 개행은 현재 미지원)
        string[] rows = raw.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        if (rows.Length <= HEADER_ROW)
        {
            Log($"  [ERR] 헤더가 없습니다.");
            return (null, null, null);
        }

        string[] headers   = ParseCsvRow(rows[HEADER_ROW]);
        string[] typeHints = (rows.Length > TYPE_ROW) ? ParseCsvRow(rows[TYPE_ROW]) : null;

        return (headers, typeHints, rows);
    }

    /// <summary>CSV 한 행을 쉼표로 분리 (따옴표 감싼 필드 대응)</summary>
    private string[] ParseCsvRow(string row)
    {
        var result = new List<string>();
        bool inQuote = false;
        var cur = new StringBuilder();

        for (int i = 0; i < row.Length; i++)
        {
            char c = row[i];
            if (c == '"')
            {
                if (inQuote && i + 1 < row.Length && row[i + 1] == '"')
                { cur.Append('"'); i++; }   // escaped quote
                else inQuote = !inQuote;
            }
            else if (c == ',' && !inQuote)
            { result.Add(cur.ToString()); cur.Clear(); }
            else cur.Append(c);
        }
        result.Add(cur.ToString());
        return result.ToArray();
    }

    // ───────────────────────────── 필드 정보 ─────────────────────────
    private List<FieldDef> BuildFields(string[] headers, string[] typeHints, string[] rows)
    {
        var fields = new List<FieldDef>();
        for (int i = 0; i < headers.Length; i++)
        {
            string name = headers[i].Trim().Replace(" ", "");
            if (string.IsNullOrEmpty(name)) continue;

            string rawHint = (typeHints != null && i < typeHints.Length) ? typeHints[i].Trim() : "";
            string typeStr = !string.IsNullOrEmpty(rawHint)
                ? MapType(rawHint.ToLower())
                : InferType(rows, i);

            var field = new FieldDef { Name = name, TypeStr = typeStr };

            if (typeStr == "enum")
                field.EnumTypeName = ParseEnumTypeName(rawHint, name);

            fields.Add(field);
        }
        return fields;
    }

    // ───────────────────────────── 코드 빌더 ─────────────────────────
    private string BuildDataClass(string className, List<FieldDef> fields, string[] rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated by CsvAutoGenerator. Do not edit manually.");
        sb.AppendLine("using System;");
        sb.AppendLine();

        // enum 타입 정의 (중복 제거)
        var enumsSeen = new HashSet<string>();
        foreach (var f in fields)
        {
            if (!f.IsEnum || !enumsSeen.Add(f.EnumTypeName)) continue;

            // 해당 필드의 컬럼 인덱스 찾기
            int colIndex = fields.IndexOf(f);

            // 데이터 행 전체를 샘플링해서 최댓값 파악
            int sampleMax = 2;   // 최소 3개(0~2) 보장
            for (int r = DATA_START; r < rows.Length; r++)
            {
                string rowTrimmed = rows[r].Trim();
                if (string.IsNullOrEmpty(rowTrimmed)) continue;
                string[] vals = ParseCsvRow(rowTrimmed);
                if (colIndex >= vals.Length) continue;
                if (int.TryParse(vals[colIndex].Trim(), out int v) && v > sampleMax)
                    sampleMax = v;
            }

            sb.AppendLine($"// TODO: '{f.EnumTypeName}' 멤버 이름을 실제 값으로 수정하세요.");
            sb.AppendLine($"public enum {f.EnumTypeName}");
            sb.AppendLine("{");
            for (int i = 0; i <= sampleMax; i++)
                sb.AppendLine($"    Value{i} = {i},");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        sb.AppendLine("[Serializable]");
        sb.AppendLine($"public class {className}");
        sb.AppendLine("{");
        foreach (var f in fields)
            sb.AppendLine($"    public {f.CSharpType} {f.Name};");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private string BuildScriptableObjectScript(string soName, string dataClassName, List<FieldDef> fields)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated by CsvAutoGenerator. Do not edit manually.");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine();
        sb.AppendLine($"[CreateAssetMenu(fileName = \"{soName}\", menuName = \"Data/{soName}\")]");
        sb.AppendLine($"public class {soName} : ScriptableObject");
        sb.AppendLine("{");
        sb.AppendLine($"    public List<{dataClassName}> dataList = new List<{dataClassName}>();");
        sb.AppendLine();

        // 키 필드: 이름에 "id" 포함 우선, 없으면 첫 번째 int, 없으면 첫 번째 필드
        var keyField = fields.Find(f => f.Name.ToLower().Contains("id") && f.TypeStr == "int")
                    ?? fields.Find(f => f.TypeStr == "int")
                    ?? fields[0];

        sb.AppendLine($"    public {dataClassName} GetById({keyField.CSharpType} id)");
        sb.AppendLine($"        => dataList.Find(x => x.{keyField.Name} == id);");
        sb.AppendLine("}");
        return sb.ToString();
    }

    // ───────────────────────────── SerializedProperty 값 설정 ─────────
    private void SetSerializedValue(SerializedProperty prop, FieldDef field, string rawVal)
    {
        // enum: SerializedPropertyType.Enum → enumValueIndex에 int값 직접 대입
        if (field.IsEnum)
        {
            prop.enumValueIndex = int.TryParse(rawVal, out int ev) ? ev : 0;
            return;
        }

        switch (field.TypeStr)
        {
            case "int":
                prop.intValue = int.TryParse(rawVal, out int iv) ? iv : 0;
                break;
            case "float":
                prop.floatValue = float.TryParse(rawVal,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float fv) ? fv : 0f;
                break;
            case "bool":
                prop.boolValue = rawVal.ToLower() == "true" || rawVal == "1";
                break;
            default:
                prop.stringValue = rawVal;
                break;
        }
    }

    // ───────────────────────────── 유틸 ──────────────────────────────
    private string[] GetCsvFiles()
    {
        string absFolder = ToAbsPath(_csvFolderPath);
        if (!Directory.Exists(absFolder))
        {
            Log($"[ERR] CSV 폴더를 찾을 수 없습니다: {_csvFolderPath}");
            return null;
        }
        var allFiles = Directory.GetFiles(absFolder, "*.csv", SearchOption.AllDirectories);
        if (allFiles.Length == 0) { Log("[ERR] CSV 파일이 없습니다."); return null; }

        // 제외 목록 필터링
        var filtered = new List<string>();
        foreach (var f in allFiles)
        {
            string name = Path.GetFileNameWithoutExtension(f);
            if (_excludeMap.TryGetValue(name, out bool excluded) && excluded)
            {
                Log($"  [SKIP] {name}.csv (제외 목록)");
                continue;
            }
            filtered.Add(f);
        }

        if (filtered.Count == 0) { Log("[ERR] 처리할 CSV 파일이 없습니다 (전부 제외됨)."); return null; }
        return filtered.ToArray();
    }

    /// <summary>CSV 폴더를 스캔해서 _excludeMap 갱신. 새 파일은 false(제외 안 함)로 추가, 기존 설정은 유지.</summary>
    private void RefreshExcludeMap()
    {
        LoadExcludeMap();   // EditorPrefs에서 기존 설정 복원

        string absFolder = ToAbsPath(_csvFolderPath);
        if (!Directory.Exists(absFolder))
        {
            Log($"[ERR] CSV 폴더를 찾을 수 없습니다: {_csvFolderPath}");
            return;
        }

        var files = Directory.GetFiles(absFolder, "*.csv", SearchOption.AllDirectories);
        // 새로 발견된 파일은 false(포함)로 추가
        foreach (var f in files)
        {
            string name = Path.GetFileNameWithoutExtension(f);
            if (!_excludeMap.ContainsKey(name))
                _excludeMap[name] = false;
        }
        // 더 이상 존재하지 않는 파일은 목록에서 제거
        var toRemove = new List<string>();
        foreach (var key in _excludeMap.Keys)
        {
            bool found = false;
            foreach (var f in files)
                if (Path.GetFileNameWithoutExtension(f) == key) { found = true; break; }
            if (!found) toRemove.Add(key);
        }
        foreach (var key in toRemove) _excludeMap.Remove(key);

        SaveExcludeMap();
        _showExcludeList = true;
        Repaint();
    }

    private void SaveExcludeMap()
    {
        // 제외(true)인 파일명만 "|" 로 이어서 저장
        var excluded = new List<string>();
        foreach (var kv in _excludeMap)
            if (kv.Value) excluded.Add(kv.Key);
        EditorPrefs.SetString(PREF_EXCLUDED, string.Join("|", excluded));
    }

    private void LoadExcludeMap()
    {
        string saved = EditorPrefs.GetString(PREF_EXCLUDED, "");
        var excludedSet = new HashSet<string>(
            saved.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));

        // 기존 _excludeMap의 체크 상태를 저장된 값으로 갱신
        var keys = new List<string>(_excludeMap.Keys);
        foreach (var key in keys)
            _excludeMap[key] = excludedSet.Contains(key);
    }

    /// <summary>컴파일된 어셈블리에서 타입 검색 (네임스페이스 무관)</summary>
    private Type FindType(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = asm.GetType(typeName);
            if (t != null) return t;
        }
        return null;
    }

    private string InferType(string[] rows, int colIndex)
    {
        for (int r = DATA_START; r < Math.Min(rows.Length, DATA_START + 5); r++)
        {
            string[] vals = ParseCsvRow(rows[r]);
            if (colIndex >= vals.Length) continue;
            string v = vals[colIndex].Trim();
            if (string.IsNullOrEmpty(v) || v == "-") continue;

            if (int.TryParse(v, out _))   return "int";
            if (float.TryParse(v, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _)) return "float";
            if (bool.TryParse(v, out _))  return "bool";
            return "string";
        }
        return "string";
    }

    private string MapType(string raw)
    {
        // "enum:ElementType" 형식 처리 — 타입명 부분은 BuildFields에서 따로 파싱
        if (raw.StartsWith("enum")) return "enum";

        return raw switch
        {
            "int"    => "int",
            "float"  => "float",
            "bool"   => "bool",
            "string" => "string",
            _        => "string"
        };
    }

    /// <summary>타입힌트 문자열에서 enum 타입명 추출. "enum:ElementType" → "ElementType"</summary>
    private string ParseEnumTypeName(string typeHint, string fieldName)
    {
        // 콜론 뒤에 이름이 있으면 그것을 사용, 없으면 필드명 기반으로 자동 생성
        int colon = typeHint.IndexOf(':');
        if (colon >= 0 && colon < typeHint.Length - 1)
        {
            string name = typeHint.Substring(colon + 1).Trim();
            if (!string.IsNullOrEmpty(name)) return name;
        }
        // 자동 생성: 필드명 PascalCase + "Type" e.g. "element" → "ElementType"
        return ToPascalCase(fieldName) + "Type";
    }

    private string ToPascalCase(string s)
    {
        var parts = s.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();
        foreach (var p in parts)
            sb.Append(char.ToUpper(p[0]) + p.Substring(1));  // 나머지는 ToLower 없이 유지
        return sb.ToString();
    }

    private string ToAbsPath(string relativePath)
    {
        string rel = relativePath.TrimStart('/').TrimStart('\\');
        if (rel.StartsWith("Assets/") || rel.StartsWith("Assets\\"))
            rel = rel.Substring("Assets/".Length);
        return Path.Combine(Application.dataPath, rel);
    }

    private void EnsureDirectory(string relativePath)
    {
        string abs = ToAbsPath(relativePath);
        if (!Directory.Exists(abs))
        {
            Directory.CreateDirectory(abs);
            Log($"  폴더 생성: {relativePath}");
        }
    }

    private void WriteFile(string folderPath, string fileName, string content)
    {
        string abs = Path.Combine(ToAbsPath(folderPath), fileName);
        if (File.Exists(abs) && !_overwriteExisting)
        {
            Log($"  [SKIP] {fileName} (이미 존재, 덮어쓰기 OFF)");
            return;
        }
        File.WriteAllText(abs, content, Encoding.UTF8);
    }

    private void Log(string msg) => _log.Add(msg);

    class FieldDef
    {
        public string Name;
        public string TypeStr;
        public string EnumTypeName;   // TypeStr == "enum" 일 때만 사용, e.g. "ElementType"

        public bool IsEnum => TypeStr == "enum";
        /// <summary>코드에 쓸 실제 C# 타입명 반환</summary>
        public string CSharpType => IsEnum ? EnumTypeName : TypeStr;
    }
}