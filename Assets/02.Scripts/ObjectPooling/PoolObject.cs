using UnityEngine;

public class PoolObject : MonoBehaviour
{
    public string poolTag; // 매니저에 등록한 태그와 동일하게 설정
    public float autoReleaseTime = 2f; // 자동으로 돌아갈 시간

    void OnEnable()
    {
        // 꺼내지자마자 n초 뒤에 다시 풀로 돌아가도록 예약
        Invoke(nameof(ReturnToPool), autoReleaseTime);
    }

    void OnDisable()
    {
        // 비활성화될 때 예약 취소 (버그 방지)
        CancelInvoke();
    }

    public void ReturnToPool()
    {
        // 매니저에게 반납
        ObjectPoolManager.Instance.Release(poolTag, gameObject);
    }
}
