using System.Collections.Generic;

public static class SubTagRegistry  // 서브 태그 ID -> Flag 변환기
{
    // Key : ID, Value : Flag
    private static Dictionary<int, int> _idToFlag = new Dictionary<int, int>();

    // 초기화 (아마 파서 통해서 들어올듯)
    public static void Init(List<int> allSubTags)
    {
        // 모든 서브 태그 순회
        for(int i = 0; i < allSubTags.Count; i++)
        {
            // 서브 태그 id 가 등록되어 있지 않다면
            if (_idToFlag.ContainsKey(allSubTags[i]) == false)
            {
                // 서브 태그 id 에 i 플래그 추가
                _idToFlag.Add(allSubTags[i], 1 << i);
            }
        }
    }

    // 서브 태그를 Key로 사용해 플래그 반환
    // 등록되어 있지 않다면 0 반환
    public static int GetFlag(int subTag)
    {
        return _idToFlag.TryGetValue(subTag, out int flag) ? flag : 0;
    }
}
