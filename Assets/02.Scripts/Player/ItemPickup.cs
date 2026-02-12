using System;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ItemPickup : MonoBehaviour
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


    public void SetColliderRange(float range) => _myCollider.radius = range;
}
