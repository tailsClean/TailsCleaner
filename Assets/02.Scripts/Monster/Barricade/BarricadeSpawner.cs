using UnityEngine;
using System.Collections;

public class BarricadeSpawner : MonoBehaviour
{
    // --- BossMonster에서 참조하는 모든 Enum 정의 (에러 해결 핵심) ---
    public enum BarricadeShape { Circle, Rectangle }
    public enum InteractionType { SolidWall, PassableWithDamage, BlockedWithDamage }
    public enum SpawnLocation { Player, Boss, Both, None }

    [Header("Prefabs")]
    public GameObject barricadePrefab; // 실제 벽 오브젝트
    public GameObject warningPrefab;   // 생성 전 예고 이펙트

    [Header("Settings")]
    public float warningDuration = 1.5f; // 예고 시간이 지난 후 벽 생성

    [Header("Random Spawn Settings (None)")]
    [Tooltip("무작위 생성 시 보스로부터의 최소 거리")]
    public float minRandomRadius = 5f;
    [Tooltip("무작위 생성 시 보스로부터의 최대 거리")]
    public float maxRandomRadius = 10f;

    public Vector2 GetSpawnPosition(SpawnLocation location, Transform player, Transform boss)
    {
        if (player == null || boss == null) return Vector2.zero;

        switch (location)
        {
            case SpawnLocation.Player:
                // 플레이어 중심
                return player.position;

            case SpawnLocation.Boss:
                // 보스 중심
                return boss.position;

            case SpawnLocation.Both:
                // 플레이어와 보스의 중간 지점
                return Vector2.Lerp(player.position, boss.position, 0.5f);

            case SpawnLocation.None:
                // 무작위: 보스 기준 인스펙터에서 설정한 반경 내
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                float randomDist = Random.Range(minRandomRadius, maxRandomRadius);
                return (Vector2)boss.position + (randomDir * randomDist);

            default:
                return boss.position;
        }
    }

    public void SpawnBarricade(Vector2 pos, BarricadeShape shape, Vector2 size, float duration, InteractionType interaction, float bossPower)
    {
        StartCoroutine(BarricadeRoutine(pos, shape, size, duration, interaction, bossPower));
    }

    private IEnumerator BarricadeRoutine(Vector2 pos, BarricadeShape shape, Vector2 size, float duration, InteractionType interaction, float bossPower)
    {
        // 1. 예고 단계 (Warning)
        if (warningPrefab != null)
        {
            GameObject warning = Instantiate(warningPrefab, pos, Quaternion.identity);
            Set2DScale(warning.transform, shape, size);

            var warningScript = warning.GetComponent<BarricadeObject>();
            if (warningScript != null)
            {
                warningScript.Setup(warningDuration, interaction, shape, size, bossPower);
            }
            else
            {
                Destroy(warning, warningDuration);
            }

            yield return new WaitForSeconds(warningDuration);
            if (warning != null) Destroy(warning);
        }

        // 실제 생성 단계 (Barricade)
        if (barricadePrefab != null)
        {
            GameObject barricade = Instantiate(barricadePrefab, pos, Quaternion.identity);
            Set2DScale(barricade.transform, shape, size);

            BarricadeObject objScript = barricade.GetComponent<BarricadeObject>();
            if (objScript != null)
            {
                
                objScript.Setup(duration, interaction, shape, size, bossPower);
            }
        }
    }
    private void Set2DScale(Transform target, BarricadeShape shape, Vector2 size)
    {
        target.localScale = Vector3.one;

        if (shape == BarricadeShape.Rectangle)
        {
            // 사각형 그룹 찾기
            Transform rectGroup = target.Find("RectGroup");
            if (rectGroup == null) return;

            // 사각형일 때는 원형 그룹 끄기
            target.Find("CircleGroup")?.gameObject.SetActive(false);
            rectGroup.gameObject.SetActive(true);

            Transform t = rectGroup.Find("Top");
            Transform b = rectGroup.Find("Bottom");
            Transform l = rectGroup.Find("Left");
            Transform r = rectGroup.Find("Right");

            if (t != null && b != null && l != null && r != null)
            {
                float thick = 0.2f; // 벽 두께
                t.localPosition = new Vector2(0, size.y / 2f);
                b.localPosition = new Vector2(0, -size.y / 2f);
                l.localPosition = new Vector2(-size.x / 2f, 0);
                r.localPosition = new Vector2(size.x / 2f, 0);

                t.localScale = new Vector2(size.x + thick, thick);
                b.localScale = new Vector2(size.x + thick, thick);
                l.localScale = new Vector2(thick, size.y + thick);
                r.localScale = new Vector2(thick, size.y + thick);
            }
        }
        else // Circle (도넛 모양)
        {
            // 원형 그룹 찾기
            Transform circleGroup = target.Find("CircleGroup");
            if (circleGroup == null) return;

            // 원형일 때는 사각형 그룹 끄기
            target.Find("RectGroup")?.gameObject.SetActive(false);
            circleGroup.gameObject.SetActive(true);

            Transform visual = circleGroup.Find("Wall_Visual");
            Transform mask = circleGroup.Find("Hole_Mask");

            if (visual != null && mask != null)
            {
                float diameter = size.x;
                visual.localScale = new Vector2(diameter, diameter);
                mask.localScale = new Vector2(diameter * 0.9f, diameter * 0.9f); // 90% 크기로 구멍
            }
        }
    }
}