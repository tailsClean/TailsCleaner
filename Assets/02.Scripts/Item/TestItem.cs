using System.Collections;
using UnityEngine;


public class TestItem : MonoBehaviour, IPickable
{
    public Transform MyTransform => transform;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerBase>(out var player))
            Destroy(gameObject);
    }
}
