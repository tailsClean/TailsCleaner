using UnityEngine;
using System;

public class PoolObject : MonoBehaviour
{
    // 매니저의 반납 함수(Release)를 연결하는 액션
    private Action<GameObject> _returnAction;

    // 매니저가 오브젝트를 풀에서 꺼낼 때 딱 한 번 호출하여 반납 통로를 설정
    public void Setup(Action<GameObject> returnAction)
    {
        _returnAction = returnAction;
    }

    // 오브젝트 스스로 사라져야 할 때 이 함수 호출하면 됩니다
    public void ReturnToPool()
    {
        // 매니저에게 나를 다시 넣어달라고 요청 (Invoke 없이 즉시 실행)
        _returnAction?.Invoke(this.gameObject);
    }
}