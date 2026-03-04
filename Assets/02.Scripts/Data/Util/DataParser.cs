using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public static class DataParser
{
    static readonly int DATA_START_IDX = 3;
    static readonly int DATA_HEADER_IDX = 1;

    public static List<T> Parse<T>(string filename) where T : new()
    {
        filename = filename.ToLower();

        List<T> data = new List<T>();

        //csv파일가져오기
        TextAsset csv = Resources.Load<TextAsset>($"Data/CSV/{filename}");
        
        if (csv == null)
        {
            //Debug.LogWarning($"csv 파일이 없습니다. Data/{filename}");
            return null;
        }

       
        string[] rows = csv.text.Trim().Split(new string[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
        if (rows.Length < DATA_START_IDX)
        {
            //Debug.LogWarning($"데이터가 없습니다.");
            return data;
        }

        string[] headers = rows[DATA_HEADER_IDX].Split(',');

        for(int i = DATA_START_IDX; i < rows.Length; i++)
        {
            string[] values = rows[i].Split(',');
            //빈값이 들어오면 패스
            if (string.IsNullOrEmpty(values[0].Trim())) continue;

            //제목의 개수와 데이터의 개수가 다르면 오류이므로 패스!
            if (values.Length != headers.Length) continue;
            
            T item = new T();
             //T클래스에 선언된 변수들을 배열 형태로 반환
            //BindingFlags.Public : 접근제어가 public인것만 찾음
            //BindingFlags.Instance : static 변수가 아닌, 객체를 생성해야 메모리가 올라가는 일반 인스턴스 변수만 찾음.
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (int j = 0; j < headers.Length; j++)
            {
                string header = headers[j].Trim().Replace(" ","");
                string value = values[j].Trim();

                //Debug.Log($"{j} : {header},{value}");

                //T클래스의 변수명과 csv파일의 헤더명이 같은지
                //if(filename.Equals("debuff"))
                //Debug.Log($"{header} : {fields[j].Name}");
                //StringComparison.OrdinalIgnoreCase : 대소문자 구분하지않고 비교하는 옵션
                FieldInfo field = Array.Find(fields, f => f.Name.Equals(header, StringComparison.OrdinalIgnoreCase));

                //Debug.Log(field);

                if (field != null)
                {
                    //빈 문자열이거나 "-" 인 경우 예외 처리
                    bool isInvalidValue = string.IsNullOrEmpty(value) || value == "-";

                    object convertedValue;

                    if (isInvalidValue)
                    {
                        //타입에 따라 기본값 할당
                        if (field.FieldType == typeof(string))
                        {
                            //스트링은 빈 값으로
                            convertedValue = string.Empty; 
                        }
                        else if (field.FieldType == typeof(int) || field.FieldType == typeof(float))
                        {
                            //숫자형은 -1로
                            convertedValue = -1; 
                        }
                        else
                        {
                            //그 외 타입은 시스템 기본값
                            convertedValue = field.FieldType.IsValueType ? Activator.CreateInstance(field.FieldType) : null;
                        }
                    }
                    else
                    {
                        try
                        {
                            //enum 처리 추가
                            if(field.FieldType.IsEnum)
                            {
                                convertedValue = Enum.Parse(field.FieldType, value);
                            }
                            else
                            {
                                convertedValue = Convert.ChangeType(value, field.FieldType);
                            }
        
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"{header} 필드 형변환 실패: {value} (Error: {ex.Message})");
                            continue;
                        }
                    }

                    field.SetValue(item, convertedValue);
                }
            }
            data.Add(item);
        }
        return data;
    }

}
          

    

