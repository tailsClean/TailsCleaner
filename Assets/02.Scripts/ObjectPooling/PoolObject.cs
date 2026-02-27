using UnityEngine;

public class PoolObject : MonoBehaviour
{
    public string poolTag;
    public float autoReleaseTime = 2f;

    void OnDisable()
    {
        // 풀로 돌아갈 때(꺼질 때) 모든 예약된 Invoke를 취소합니다.
        CancelInvoke();
    }

    // 매니저가 소환 직후 이 함수를 직접 호출
    public void ResetReleaseTimer(float newLifeTime = -1f)
    {
        if (newLifeTime > 0)
        {
            autoReleaseTime = newLifeTime;
        }

        CancelInvoke(nameof(ReturnToPool));
        Invoke(nameof(ReturnToPool), autoReleaseTime);
    }

    public void ReturnToPool()
    {
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.Release(poolTag, gameObject);
        }
    }
}