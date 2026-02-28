using System;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ItemPickupSystem : MonoBehaviour
{
    private CircleCollider2D _myCollider;

    public event Action<IPickable> OnEnterPickupRange;



    private void Awake()
    {
        _myCollider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        _myCollider.isTrigger = true;
    }



    // 픽업가능한 아이템(경험치)이 범위에 들어오면 프레임마다 실행
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.TryGetComponent<IPickable>(out var item))
            OnEnterPickupRange?.Invoke(item);
    }


    // 주위 아이템(경험치) 끌어모으는 메서드
    public void ItemPickup(Transform playerTr, IPickable item)
    {
        Vector2 itemPos = item.MyTransform.position;
        Vector2 myPos = playerTr.position;

        // 마지막 인자값은 끌어당기는 속도
        item.MyTransform.position = Vector2.MoveTowards(itemPos, myPos, 1f * Time.deltaTime);
    }


    public void SetColliderRange(float range) => _myCollider.radius = range;
}
