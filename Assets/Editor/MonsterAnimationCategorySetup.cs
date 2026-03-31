using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

public class MonsterMultipleSpriteAutoSetup
{
    [MenuItem("Tools/Addressables/Build Monster Clips from Multiple Sprites")]
    public static void Execute()
    {
        string rootPath = "Assets/00.Resources/Monster";
        string[] categories = { "02. Boss", "03. Elite", "06. Normal", "07. Special" };
        string groupName = "MonsterResources";

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return;
        var group = settings.FindGroup(groupName) ?? settings.CreateGroup(groupName, false, false, true, null);

        foreach (string category in categories)
        {
            string categoryPath = Path.Combine(rootPath, category);
            if (!Directory.Exists(categoryPath)) continue;

            // 1. 해당 폴더의 모든 Texture2D(이미지 파일)를 찾습니다.
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { categoryPath });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(assetPath); // 예: elite_move_1-1

                // 2. 해당 파일 안에 들어있는 모든 'Sprite'들을 가져옵니다. (자식 스프라이트들)
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                List<Sprite> sprites = assets.OfType<Sprite>()
                                             .OrderBy(s => GetNumericSuffix(s.name)) // 이름 끝 숫자 순 정렬
                                             .ToList();

                if (sprites.Count == 0) continue;

                // 3. .anim 파일 경로 설정 (이미지 파일과 같은 폴더에 생성)
                string clipPath = Path.Combine(Path.GetDirectoryName(assetPath), fileName + ".anim");

                // 4. 애니메이션 클립 생성/업데이트
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null)
                {
                    clip = new AnimationClip();
                    AssetDatabase.CreateAsset(clip, clipPath);
                }

                // 루프 및 프레임 속도 설정 (12fps)
                var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
                clipSettings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
                clip.frameRate = 12;

                // 스프라이트 키프레임 주입
                EditorCurveBinding spriteBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
                ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
                for (int i = 0; i < sprites.Count; i++)
                {
                    keyframes[i] = new ObjectReferenceKeyframe { time = i / clip.frameRate, value = sprites[i] };
                }
                AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

                // 5. 어드레서블 등록 (주소 = 파일 이름, 예: elite_move_1-1)
                string clipGuid = AssetDatabase.AssetPathToGUID(clipPath);
                var entry = settings.CreateOrMoveEntry(clipGuid, group);
                entry.address = fileName;

                Debug.Log($"[{category}] 애니메이션 생성 및 주소 등록 완료: {fileName}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Multiple Sprite 기반 모든 몬스터 애니메이션 설정 완료!");
    }

    // 이름 끝의 숫자(예: _0, _1)를 추출해서 정렬용으로 사용
    private static int GetNumericSuffix(string name)
    {
        string suffix = System.Text.RegularExpressions.Regex.Match(name, @"\d+$").Value;
        return int.TryParse(suffix, out int result) ? result : 0;
    }
}