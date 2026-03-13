using UnityEngine;

public class BarricadeObject : MonoBehaviour
{
    public GameObject destroyEffect; // 파괴 시 재생할 이펙트 (인스펙터에서 할당)
    private float damagePower;
    private BarricadeSpawner.InteractionType interaction;
    private Vector2 barricadeSize;
    private BarricadeSpawner.BarricadeShape shape;

    private bool isActive = true; // Destroy 후 위치 보정 중지
    public void Setup(float duration, BarricadeSpawner.InteractionType interaction, BarricadeSpawner.BarricadeShape shape, Vector2 size, float bossPower)
    {
        this.interaction = interaction;
        this.damagePower = bossPower;

        this.shape = shape;
        this.barricadeSize = size;

        // 1. 모양에 따라 그룹 활성화/비활성화 (프리팹 이름과 일치해야 함)
        Transform rectGroup = transform.Find("RectGroup");
        Transform circleGroup = transform.Find("CircleGroup");

        if (rectGroup != null) rectGroup.gameObject.SetActive(shape == BarricadeSpawner.BarricadeShape.Rectangle);
        if (circleGroup != null) circleGroup.gameObject.SetActive(shape == BarricadeSpawner.BarricadeShape.Circle);

        // 2. 원형일 경우 부모의 EdgeCollider를 동그랗게 재설정
        if (shape == BarricadeSpawner.BarricadeShape.Circle)
        {
            UpdateCircleCollider(size.x); // size.x를 지름으로 사용
        }

        // 3. 자식들에게 붙은 모든 Collider2D를 가져와서 트리거 설정 (사각형 벽들 포함)
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        foreach (var col in colliders)
        {
            if (interaction == BarricadeSpawner.InteractionType.PassableWithDamage)
            {
                col.isTrigger = true;  // 통과 가능
            }
            else
            {
                col.isTrigger = false; // 딱딱한 벽 (Blocked, Solid 둘 다)
            }

            // Debug.Log($"{col.gameObject.name}의 트리거 상태: {col.isTrigger}");
        }

        // 4. 지정된 시간 후 파괴
        isActive = true;
        Destroy(gameObject, duration);
    }

    private void UpdateCircleCollider(float diameter)
    {
        // 최상위 부모(Master)에 붙은 EdgeCollider2D를 가져옵니다.
        EdgeCollider2D edge = GetComponent<EdgeCollider2D>();

        if (edge == null) return;

        edge.edgeRadius = 0.2f; // 충돌 두께 설정

        float radius = diameter / 2f;
        int pointsCount = 40;
        Vector2[] points = new Vector2[pointsCount + 1];

        for (int i = 0; i <= pointsCount; i++)
        {
            float angle = i * 2 * Mathf.PI / pointsCount;
            points[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        }
        edge.points = points;
    }

    // --- 대미지 판정 로직 ---
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;

        // 갇힘 위치 보정은 오직 BlockedWithDamage일 때만
        if (interaction == BarricadeSpawner.InteractionType.BlockedWithDamage)
        {
            if (shape == BarricadeSpawner.BarricadeShape.Circle)
                PushPlayerInsideCircle(other.transform);
            else
                PushPlayerInsideRectangle(other.transform);
        }

        ApplyDamage(other.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) ApplyDamage(collision.gameObject);
    }

    private void ApplyDamage(GameObject playerObj)
    {
        // 로그 1: 일단 뭔가 닿긴 했는지 확인
        // Debug.Log($"<color=yellow>[충돌 감지]</color> 닿은 물체: {playerObj.name}, 태그: {playerObj.tag}");

        // 1. SolidWall 체크
        if (interaction == BarricadeSpawner.InteractionType.SolidWall)
        {
            // Debug.Log("상태: SolidWall이라 데미지 없음");
            return;
        }

        // 2. 컴포넌트 체크
        if (playerObj.TryGetComponent<IDamageable>(out var damageable))
        {
            float finalDamage = damagePower * Time.deltaTime;
            damageable.TakeDamage(finalDamage);

            // 로그 2: 실제로 데미지 함수를 호출했는지 확인
            Debug.Log($"<color=red>[데미지 전달]</color> 파워: {damagePower}, 최종딜: {finalDamage}");
        }
        else
        {
            // 로그 3: IDamageable이 없는 경우
            // Debug.LogWarning($"{playerObj.name}에 IDamageable(체력 스크립트)이 없습니다!");
        }
    }

    private void PushPlayerInsideCircle(Transform player)
    {
        float radius = barricadeSize.x / 2f;

        Vector2 center = transform.position;
        Vector2 playerPos = player.position;

        float dist = Vector2.Distance(center, playerPos);

        if (dist > radius)
        {
            Vector2 dir = (playerPos - center).normalized;
            player.position = center + dir * (radius - 0.05f);
        }
    }

    private void PushPlayerInsideRectangle(Transform player)
    {
        Vector2 center = transform.position;
        Vector2 pos = player.position;

        float halfX = barricadeSize.x / 2f;
        float halfY = barricadeSize.y / 2f;

        float left = center.x - halfX;
        float right = center.x + halfX;
        float bottom = center.y - halfY;
        float top = center.y + halfY;

        float push = 0.05f;

        if (pos.x < left)
            pos.x = left + push;
        else if (pos.x > right)
            pos.x = right - push;

        if (pos.y < bottom)
            pos.y = bottom + push;
        else if (pos.y > top)
            pos.y = top - push;

        player.position = pos;
    }

    private void OnDestroy()
    {
        isActive = false;
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }
    }
}