using System.IO;
using System.Linq;
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

        var group = settings.FindGroup(groupName);
        if (group == null)
        {
            Debug.LogError($"Addressables group not found: {groupName}");
            return;
        }

        int removedCount = 0;
        int registeredCount = 0;
        int skippedMultipleCount = 0;

        // 1. 기존 그룹 엔트리 중 Monster 폴더 아래 Multiple 스프라이트는 먼저 제거
        var oldEntries = group.entries.ToList();
        foreach (var entry in oldEntries)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);
            if (string.IsNullOrEmpty(assetPath))
                continue;

            if (!assetPath.StartsWith(rootFolder))
                continue;

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                continue;

            if (importer.textureType == TextureImporterType.Sprite &&
                importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                settings.RemoveAssetEntry(entry.guid);
                removedCount++;
                Debug.Log($"[기존 엔트리 제거] Multiple Sprite: {assetPath}");
            }
        }

        // 2. Monster 폴더 아래 Texture2D 다시 스캔
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { rootFolder });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                continue;

            // Sprite 아닌 건 무시
            if (importer.textureType != TextureImporterType.Sprite)
                continue;

            // Multiple이면 Addressables 등록 안 함
            if (importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                skippedMultipleCount++;
                Debug.Log($"[제외] Multiple Sprite: {assetPath}");
                continue;
            }

            // Single만 등록
            string addressName = Path.GetFileNameWithoutExtension(assetPath);

            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = addressName;

            registeredCount++;
            Debug.Log($"[등록 완료] {assetPath} -> {addressName}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"몬스터 스프라이트 Addressables 자동 설정 완료 / Group: {groupName}\n" +
            $"- 기존 Multiple 제거: {removedCount}\n" +
            $"- 등록된 Single Sprite: {registeredCount}\n" +
            $"- 제외된 Multiple Sprite: {skippedMultipleCount}"
        );
    }
}