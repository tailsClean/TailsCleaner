using UnityEngine;


public class ItemExp : MonoBehaviour, IPickable
{
    [SerializeField] private int _expPoint = 10;
    [SerializeField] private FloatEventChannelSO _onPickupExp;

    public Transform MyTransform => transform;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerBase>(out var player))
        {
            _onPickupExp.OnStartEvent(_expPoint);
            Destroy(gameObject);
        }
    }

    
}
