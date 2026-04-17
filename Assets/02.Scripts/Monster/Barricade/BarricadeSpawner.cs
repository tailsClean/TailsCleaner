using UnityEngine;
using System.Collections;

public class BarricadeSpawner : MonoBehaviour
{
    // --- BossMonster에서 참조하는 모든 Enum 정의 ---
    public enum BarricadeShape { Circle, Rectangle }
    public enum InteractionType { SolidWall, PassableWithDamage, BlockedWithDamage }
    public enum SpawnLocation { Player, Boss, Both, None }

    [Header("Prefabs")]
    public GameObject barricadePrefab;   // 실제 벽 오브젝트
    public GameObject warningPrefab;     // 생성 전 예고 이펙트
    public PoolObject circleTilePrefab;  
    public PoolObject warningTilePrefab;

    [Header("Settings")]
    public float warningDuration = 1.5f; // 예고 시간이 지난 후 벽 생성

    [Header("Visual Settings")]
    [Tooltip("원형 타일 사이 간격")]
    public float tileSpacing = 0.4f;

    [Tooltip("사각형 벽 콜라이더 두께")]
    public float rectangleThickness = 0.2f;

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
                return player.position;

            case SpawnLocation.Boss:
                return boss.position;

            case SpawnLocation.Both:
                return Vector2.Lerp(player.position, boss.position, 0.5f);

            case SpawnLocation.None:
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                float randomDist = Random.Range(minRandomRadius, maxRandomRadius);
                return (Vector2)boss.position + (randomDir * randomDist);

            default:
                return boss.position;
        }
    }

    public void SpawnBarricade(
        Vector2 pos,
        BarricadeShape shape,
        Vector2 size,
        float duration,
        InteractionType interaction,
        float bossPower,
        float castTime)
    {
        StartCoroutine(BarricadeRoutine(pos, shape, size, duration, interaction, bossPower, castTime));
    }

    private IEnumerator BarricadeRoutine(
    Vector2 pos,
    BarricadeShape shape,
    Vector2 size,
    float duration,
    InteractionType interaction,
    float bossPower,
    float castTime)
    {
        if (warningPrefab != null)
        {
            GameObject warning = Instantiate(warningPrefab, pos, Quaternion.identity);

            BuildBarricadeVisual(warning.transform, shape, size, warningTilePrefab);

            Collider2D[] warningCols = warning.GetComponentsInChildren<Collider2D>(true);
            foreach (var col in warningCols)
            {
                col.enabled = false;
            }

            float waitTime = castTime > 0f ? castTime : warningDuration;
            yield return new WaitForSeconds(waitTime);

            if (warning != null)
            {
                warning.SetActive(false);
                Destroy(warning);
            }

            yield return null;
        }

        if (barricadePrefab != null)
        {
            Debug.Log("[BARRICADE] 생성 시작");

            GameObject barricade = Instantiate(barricadePrefab, pos, Quaternion.identity);
            Debug.Log($"[BARRICADE] instantiate 성공: {barricade.name}");

            Debug.Log($"[BARRICADE] circleTilePrefab 연결됨? {circleTilePrefab != null}");
            Debug.Log($"[BARRICADE] RectGroup 있음? {barricade.transform.Find("RectGroup") != null}");
            Debug.Log($"[BARRICADE] CircleGroup 있음? {barricade.transform.Find("CircleGroup") != null}");

            BuildBarricadeVisual(barricade.transform, shape, size, circleTilePrefab);

            BarricadeObject objScript = barricade.GetComponent<BarricadeObject>();
            Debug.Log($"[BARRICADE] BarricadeObject 있음? {objScript != null}");

            if (objScript != null)
            {
                objScript.Setup(duration, interaction, shape, size, bossPower);
                Debug.Log("[BARRICADE] Setup 호출 완료");
            }
        }
        else
        {
            Debug.LogWarning("[BARRICADE] barricadePrefab이 비어 있음");
        }
    }

    private void BuildBarricadeVisual(Transform target, BarricadeShape shape, Vector2 size, PoolObject tilePrefab)
    {
        if (shape == BarricadeShape.Rectangle)
        {
            BuildRectangleVisual(target, size, tilePrefab);
        }
        else
        {
            BuildCircleVisual(target, size, tilePrefab);
        }
    }

    private void BuildRectangleVisual(Transform target, Vector2 size, PoolObject tilePrefab)
    {
        Transform rectGroup = target.Find("RectGroup");
        if (rectGroup == null) return;

        Transform circleGroup = target.Find("CircleGroup");
        if (circleGroup != null)
            circleGroup.gameObject.SetActive(false);

        rectGroup.gameObject.SetActive(true);

        Transform top = rectGroup.Find("Top");
        Transform bottom = rectGroup.Find("Bottom");
        Transform left = rectGroup.Find("Left");
        Transform right = rectGroup.Find("Right");

        float thick = rectangleThickness;
        float spacing = tileSpacing;

        SetupRectSide(top, new Vector2(0f, size.y / 2f), new Vector2(size.x + thick, thick), new Vector2(-size.x / 2f, 0f), new Vector2(size.x / 2f, 0f), spacing, tilePrefab);
        SetupRectSide(bottom, new Vector2(0f, -size.y / 2f), new Vector2(size.x + thick, thick), new Vector2(-size.x / 2f, 0f), new Vector2(size.x / 2f, 0f), spacing, tilePrefab);
        SetupRectSide(left, new Vector2(-size.x / 2f, 0f), new Vector2(thick, size.y + thick), new Vector2(0f, -size.y / 2f), new Vector2(0f, size.y / 2f), spacing, tilePrefab);
        SetupRectSide(right, new Vector2(size.x / 2f, 0f), new Vector2(thick, size.y + thick), new Vector2(0f, -size.y / 2f), new Vector2(0f, size.y / 2f), spacing, tilePrefab);
    }

    private void BuildCircleVisual(Transform target, Vector2 size, PoolObject tilePrefab)
    {
        Transform circleGroup = target.Find("CircleGroup");
        if (circleGroup == null) return;

        Transform rectGroup = target.Find("RectGroup");
        if (rectGroup != null)
            rectGroup.gameObject.SetActive(false);

        circleGroup.gameObject.SetActive(true);

        Transform wallVisual = circleGroup.Find("Wall_Visual");
        if (wallVisual == null) return;

        Transform holeMask = circleGroup.Find("Hole_Mask");
        if (holeMask != null)
            holeMask.gameObject.SetActive(false);

        float radius = size.x / 2f;
        BuildRingWithCircles(wallVisual, radius, tileSpacing, tilePrefab);
    }

    private void SetupRectSide(
    Transform side,
    Vector2 localPos,
    Vector2 colliderSize,
    Vector2 lineStart,
    Vector2 lineEnd,
    float spacing,
    PoolObject tilePrefab)
    {
        if (side == null) return;

        side.localPosition = localPos;
        side.localRotation = Quaternion.identity;
        side.localScale = Vector3.one;

        BoxCollider2D col = side.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = colliderSize;
            col.offset = Vector2.zero;
        }

        BuildLineWithCircles(side, lineStart, lineEnd, spacing, tilePrefab);
    }

    private void BuildLineWithCircles(Transform parent, Vector2 start, Vector2 end, float spacing, PoolObject tilePrefab)
    {
        if (parent == null || tilePrefab == null) return;

        ClearChildren(parent);

        float distance = Vector2.Distance(start, end);
        int count = Mathf.Max(2, Mathf.RoundToInt(distance / spacing) + 1);

        for (int i = 0; i < count; i++)
        {
            float t = (count == 1) ? 0f : (float)i / (count - 1);
            Vector2 pos = Vector2.Lerp(start, end, t);

            PoolObject dot = ObjectPoolManager.Instance.Spawn(tilePrefab, Vector3.zero, Quaternion.identity);
            dot.transform.SetParent(parent, false);
            dot.transform.localPosition = pos;
            dot.transform.localRotation = Quaternion.identity;
            dot.transform.localScale = Vector3.one;
        }
    }

    private void BuildRingWithCircles(Transform parent, float radius, float spacing, PoolObject tilePrefab)
    {
        if (parent == null || tilePrefab == null) return;

        ClearChildren(parent);

        float circumference = 2f * Mathf.PI * radius;
        int count = Mathf.Max(8, Mathf.RoundToInt(circumference / spacing));

        for (int i = 0; i < count; i++)
        {
            float angle = (2f * Mathf.PI * i) / count;

            Vector2 pos = new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );

            PoolObject dot = ObjectPoolManager.Instance.Spawn(tilePrefab, Vector3.zero, Quaternion.identity);
            dot.transform.SetParent(parent, false);
            dot.transform.localPosition = pos;
            dot.transform.localRotation = Quaternion.identity;
            dot.transform.localScale = Vector3.one;
        }
    }



    private void ClearChildren(Transform parent)
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);

            if (child.TryGetComponent<PoolObject>(out var poolObj))
            {
                child.SetParent(ObjectPoolManager.Instance.transform);
                ObjectPoolManager.Instance.ReturnObject(poolObj);
            }
            else
            {
                Destroy(child.gameObject);
            }
        }
    }
}