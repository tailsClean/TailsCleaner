using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class DataParser
{
    static readonly int DATA_START_IDX = 3;
    static readonly int DATA_HEADER_IDX = 1;

    public static List<T> Parse<T>(string relativePath) where T : new()
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            Debug.LogError("[DataParser] relativePath is null or empty.");
            return null;
        }

        relativePath = relativePath.Trim().Replace("\\", "/");

        // Data/CSV 하위 상대 경로만 받도록 통일
        string loadPath = $"Data/CSV/{relativePath}";
        TextAsset csv = Resources.Load<TextAsset>(loadPath);

        if (csv == null)
        {
            Debug.LogError($"[DataParser] csv 파일이 없습니다. path={loadPath}");
            return null;
        }

        string[] rows = csv.text.Trim().Split(
            new string[] { "\r\n", "\r", "\n" },
            StringSplitOptions.None
        );

        if (rows.Length < DATA_START_IDX)
        {
            return new List<T>();
        }

        string[] headers = rows[DATA_HEADER_IDX].Split(',');
        List<T> data = new List<T>();

        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

        for (int i = DATA_START_IDX; i < rows.Length; i++)
        {
            string[] values = rows[i].Split(',');

            if (values.Length == 0 || string.IsNullOrEmpty(values[0].Trim()))
                continue;

            if (values.Length != headers.Length)
            {
                Debug.LogWarning($"[DataParser] 헤더/값 개수 불일치. path={loadPath}, row={i + 1}");
                continue;
            }

            T item = new T();

            for (int j = 0; j < headers.Length; j++)
            {
                string header = headers[j].Trim().Replace(" ", "");
                string value = values[j].Trim();

                FieldInfo field = Array.Find(fields,
                    f => f.Name.Equals(header, StringComparison.OrdinalIgnoreCase));

                if (field == null)
                    continue;

                bool isInvalidValue = string.IsNullOrEmpty(value) || value == "-";
                object convertedValue;

                if (isInvalidValue)
                {
                    if (field.FieldType == typeof(string))
                    {
                        convertedValue = string.Empty;
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        convertedValue = -1;
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        convertedValue = -1f;
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        convertedValue = false;
                    }
                    else if (field.FieldType.IsEnum)
                    {
                        convertedValue = Activator.CreateInstance(field.FieldType);
                    }
                    else
                    {
                        convertedValue = field.FieldType.IsValueType
                            ? Activator.CreateInstance(field.FieldType)
                            : null;
                    }
                }
                else
                {
                    try
                    {
                        if (field.FieldType.IsEnum)
                        {
                            // enum 숫자값/문자열 둘 다 대응
                            if (int.TryParse(value, out int enumInt))
                                convertedValue = Enum.ToObject(field.FieldType, enumInt);
                            else
                                convertedValue = Enum.Parse(field.FieldType, value, true);
                        }
                        else if (field.FieldType == typeof(bool))
                        {
                            if (value == "1")
                                convertedValue = true;
                            else if (value == "0")
                                convertedValue = false;
                            else
                                convertedValue = Convert.ChangeType(value, field.FieldType);
                        }
                        else
                        {
                            convertedValue = Convert.ChangeType(value, field.FieldType);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(
                            $"[DataParser] 형변환 실패. path={loadPath}, row={i + 1}, field={header}, value={value}, error={ex.Message}"
                        );
                        continue;
                    }
                }

                field.SetValue(item, convertedValue);
            }

            data.Add(item);
        }

        Debug.Log($"[DataParser] parsed {typeof(T).Name} count={data.Count}, path={loadPath}");
        return data;
    }
}




