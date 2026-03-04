using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

/// <summary>
/// CSV 파일을 분석해서 Data Class + ScriptableObject를 자동 생성하는 에디터 툴
/// </summary>
public class CsvAutoGenerator : EditorWindow
{
    // ───────────────────────────── 상수 ─────────────────────────────
    const int COMMENT_ROW  = 0;   // 0번째 줄 : 주석
    const int HEADER_ROW   = 1;   // 1번째 줄 : 필드명
    const int TYPE_ROW     = 2;   // 2번째 줄 : 타입 힌트 (optional)
    const int DATA_START   = 3;   // 3번째 줄부터 : 실제 데이터

    // ───────────────────────────── 상태 ─────────────────────────────
    string _csvFolderPath    = "Assets/00.Resources/Resources/Data/CSV";
    string _classOutputPath  = "Assets/02.Scripts/Data";
    string _soOutputPath     = "Assets/00.Resources/Resources/Data/ScriptableObjects";
    bool   _generateSO       = true;
    bool   _generateClass    = true;
    bool   _overwriteExisting = true;

    Vector2 scroll;
    List<string> log = new List<string>();

    // ───────────────────────────── 메뉴 ─────────────────────────────
    [MenuItem("Tools/CSV Auto Generator")]
    public static void Open() => GetWindow<CsvAutoGenerator>("CSV Auto Generator");

    // ───────────────────────────── GUI ──────────────────────────────
    void OnGUI()
    {
        GUILayout.Label("⚙ CSV 자동 생성기", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        DrawPathField("CSV 폴더 경로",    ref _csvFolderPath,   true);
        DrawPathField("Class 출력 경로",  ref _classOutputPath, true);
        DrawPathField("SO 출력 경로",     ref _soOutputPath,    true);

        EditorGUILayout.Space(6);
        _generateClass    = EditorGUILayout.Toggle("Data Class 생성",        _generateClass);
        _generateSO       = EditorGUILayout.Toggle("ScriptableObject 생성",  _generateSO);
        _overwriteExisting = EditorGUILayout.Toggle("기존 파일 덮어쓰기",     _overwriteExisting);

        EditorGUILayout.Space(10);

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("▶  Generate All", GUILayout.Height(36)))
            RunGenerate();
        GUI.backgroundColor = Color.white;

        // 로그 영역
        EditorGUILayout.Space(8);
        GUILayout.Label("Log", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(200));
        foreach (var line in log)
        {
            Color prev = GUI.color;
            if (line.StartsWith("[ERR]"))  GUI.color = new Color(1f, 0.4f, 0.4f);
            else if (line.StartsWith("[OK]")) GUI.color = new Color(0.5f, 1f, 0.5f);
            else GUI.color = Color.white;
            GUILayout.Label(line, EditorStyles.wordWrappedLabel);
            GUI.color = prev;
        }
        EditorGUILayout.EndScrollView();

        if (log.Count > 0 && GUILayout.Button("Clear Log"))
            log.Clear();
    }

    void DrawPathField(string label, ref string path, bool isFolder)
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
                // 절대 경로 → 프로젝트 상대 경로로 변환
                if (selected.StartsWith(Application.dataPath))
                    selected = "Assets" + selected.Substring(Application.dataPath.Length);
                path = selected;
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    // ───────────────────────────── 핵심 로직 ─────────────────────────
    void RunGenerate()
    {
        log.Clear();

        string absoluteCsvFolder = Path.Combine(Application.dataPath,
            _csvFolderPath.Replace("Assets/", "").Replace("Assets\\", ""));

        if (!Directory.Exists(absoluteCsvFolder))
        {
            Log($"[ERR] CSV 폴더를 찾을 수 없습니다: {_csvFolderPath}");
            return;
        }

        string[] csvFiles = Directory.GetFiles(absoluteCsvFolder, "*.csv", SearchOption.AllDirectories);
        if (csvFiles.Length == 0)
        {
            Log("[ERR] CSV 파일이 없습니다.");
            return;
        }

        EnsureDirectory(_classOutputPath);
        EnsureDirectory(_soOutputPath);

        int successCount = 0;
        foreach (string csvPath in csvFiles)
        {
            try
            {
                ProcessCsv(csvPath);
                successCount++;
            }
            catch (Exception e)
            {
                Log($"[ERR] {Path.GetFileName(csvPath)} 처리 중 오류: {e.Message}");
            }
        }

        AssetDatabase.Refresh();
        Log($"──────────────────────────────");
        Log($"[OK] 완료! {successCount}/{csvFiles.Length} 파일 처리됨");
    }

    void ProcessCsv(string csvAbsPath)
    {
        string fileName  = Path.GetFileNameWithoutExtension(csvAbsPath);
        string className = ToPascalCase(fileName);        // e.g. "monster_data" → "MonsterData"
        string soName    = className + "SO";              // e.g. "MonsterDataSO"
        string listName  = className + "Database";        // e.g. "MonsterDataDatabase"

        Log($"── {fileName}.csv 처리 중...");

        // CSV 읽기
        string rowText = File.ReadAllText(csvAbsPath, Encoding.UTF8).Trim();
        string[] rows = Regex.Split(rowText, @"\r\n|\r|\n(?=(?:[^""]*""[^""]*"")*[^""]*$)");

        if (rows.Length <= HEADER_ROW)
        {
            Log($"  [ERR] 헤더가 없습니다.");
            return;
        }

        string[] headers   = rows[HEADER_ROW].Split(',');
        string[] typeHints = (rows.Length > TYPE_ROW) ? rows[TYPE_ROW].Split(',') : null;

        // 필드 정보 수집
        var fields = new List<FieldDef>();
        for (int i = 0; i < headers.Length; i++)
        {
            string name = headers[i].Trim().Replace(" ", "");
            if (string.IsNullOrEmpty(name)) continue;

            string typeStr = (typeHints != null && i < typeHints.Length)
                ? typeHints[i].Trim().ToLower()
                : InferType(rows, i);   // 타입힌트 없으면 데이터에서 추론

            fields.Add(new FieldDef { Name = name, TypeStr = MapType(typeStr) });
        }

        // ── Class 생성
        if (_generateClass)
        {
            string classCode = BuildDataClass(className, fields);
            WriteFile(_classOutputPath, className + ".cs", classCode);
            Log($"  [OK] {className}.cs 생성");
        }

        // ── ScriptableObject 생성
        if (_generateSO)
        {
            string soCode = BuildScriptableObject(soName, listName, className, fields);
            WriteFile(_classOutputPath, soName + ".cs", soCode);
            Log($"  [OK] {soName}.cs 생성");
        }
    }

    // ───────────────────────────── 코드 빌더 ─────────────────────────

    string BuildDataClass(string className, List<FieldDef> fields)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated by CsvAutoGenerator. Do not edit manually.");
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("[Serializable]");
        sb.AppendLine($"public class {className}");
        sb.AppendLine("{");
        foreach (var f in fields)
            sb.AppendLine($"    public {f.TypeStr} {f.Name};");
        sb.AppendLine("}");
        return sb.ToString();
    }

    string BuildScriptableObject(string soName, string listName, string dataClassName, List<FieldDef> fields)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// 자동 생성 됨.");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine();
        sb.AppendLine($"[CreateAssetMenu(fileName = \"{soName}\", menuName = \"Data/{soName}\")]");
        sb.AppendLine($"public class {soName} : ScriptableObject");
        sb.AppendLine("{");
        sb.AppendLine($"    public List<{dataClassName}> dataList = new List<{dataClassName}>();");
        sb.AppendLine();

        // 키 타입 추론 (첫 번째 int 필드가 있으면 int, 없으면 string)
        var keyField = fields.Find(f => f.TypeStr == "int") ?? fields[0];
        sb.AppendLine($"    public {dataClassName} GetById({keyField.TypeStr} id)");
        sb.AppendLine($"        => dataList.Find(x => x.{keyField.Name} == id);");
        sb.AppendLine("}");
        return sb.ToString();
    }

    // ───────────────────────────── 유틸 ──────────────────────────────

    /// <summary>데이터 행을 샘플링해서 타입 추론</summary>
    string InferType(string[] rows, int colIndex)
    {
        for (int r = DATA_START; r < Math.Min(rows.Length, DATA_START + 5); r++)
        {
            string[] vals = rows[r].Split(',');
            if (colIndex >= vals.Length) continue;
            string v = vals[colIndex].Trim();
            if (string.IsNullOrEmpty(v) || v == "-") continue;

            if (int.TryParse(v, out _))    return "int";
            if (float.TryParse(v, out _))  return "float";
            if (bool.TryParse(v, out _))   return "bool";
            return "string";
        }
        return "string";
    }

    string MapType(string raw) => raw switch
    {
        "int"    => "int",
        "float"  => "float",   
        "bool"   => "bool",
        "string" => "string",
        "enum"   => "int",    // enum은 int로 처리
        _        => "string"
    };

    string ToPascalCase(string s)
    {
        var parts = s.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();
        foreach (var p in parts)
            sb.Append(char.ToUpper(p[0]) + p.Substring(1).ToLower());
        return sb.ToString();
    }

    void EnsureDirectory(string relativePath)
    {
        string abs = Path.Combine(Application.dataPath,
            relativePath.Replace("Assets/", "").Replace("Assets\\", ""));
        if (!Directory.Exists(abs))
        {
            Directory.CreateDirectory(abs);
            Log($"  폴더 생성: {relativePath}");
        }
    }

    void WriteFile(string folderPath, string fileName, string content)
    {
        string abs = Path.Combine(Application.dataPath,
            folderPath.Replace("Assets/", "").Replace("Assets\\", ""),
            fileName);

        if (File.Exists(abs) && !_overwriteExisting)
        {
            Log($"  [SKIP] {fileName} (이미 존재, 덮어쓰기 OFF)");
            return;
        }
        File.WriteAllText(abs, content, Encoding.UTF8);
    }

    void Log(string msg) => log.Add(msg);

    class FieldDef
    {
        public string Name;
        public string TypeStr;
    }
}