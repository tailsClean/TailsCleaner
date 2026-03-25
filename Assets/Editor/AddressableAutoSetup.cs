using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

public class AddressableAutoSetup
{
    [MenuItem("Tools/Addressables/Auto Setup Monster Sprites")]
    public static void SetupMonsterSprites()
    {
        string rootFolder = "Assets/00.Resources/Monster";
        string groupName = "MonsterResources";

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Settings not found.");
            return;
        }

        // MonsterResources 그룹 찾기
        var group = settings.FindGroup(groupName);
        if (group == null)
        {
            Debug.LogError($"Addressables group not found: {groupName}");
            return;
        }

        // Monster 폴더 아래 Sprite 전부 검색
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { rootFolder });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // 파일명만 추출하고 확장자 제거
            string addressName = Path.GetFileNameWithoutExtension(assetPath);

            // 그룹에 추가 또는 이동
            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = addressName;

            Debug.Log($"등록 완료: {assetPath} -> {addressName}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"몬스터 스프라이트 Addressables 자동 설정 완료 / Group: {groupName}");
    }
}