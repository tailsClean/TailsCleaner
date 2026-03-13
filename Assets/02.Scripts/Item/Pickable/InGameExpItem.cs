using UnityEngine;

public class InGameExpItem : PoolObject, IPickable
{
    [SerializeField] private int _expPoint = 10;
    [SerializeField] private FloatEventChannelSO _onPickupExp;

    public Transform MyTransform => transform;

    public void SetExp(int exp)
    {
        _expPoint = Mathf.Max(0, exp);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerBase>(out var player))
        {
            _onPickupExp.OnStartEvent(_expPoint);
            if (ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.ReturnObject(this);
            else
                Destroy(gameObject);
        }
    }
}
