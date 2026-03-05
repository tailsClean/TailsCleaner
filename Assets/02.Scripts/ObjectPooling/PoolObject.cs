using UnityEngine;

public class PoolObject : MonoBehaviour
{
    // 나중에 이 오브젝트를 다시 회수할 풀의 이름을 저장합니다.
    public string PoolKey { get; set; }

    // 오브젝트가 활성화될 때 실행될 가상 함수 (재정의 가능)
    public virtual void OnSpawn() { }

    // 풀로 돌아갈 때 실행될 가상 함수
    public virtual void OnDespawn()
    {
        gameObject.SetActive(false);
    }

    // 일정 시간 후 자동 반납 기능
    public void ReturnToPoolAfter(float delay)
    {
        Invoke(nameof(Deactivate), delay);
    }

    private void Deactivate()
    {
        ObjectPoolManager.Instance.ReturnObject(this);
    }
}